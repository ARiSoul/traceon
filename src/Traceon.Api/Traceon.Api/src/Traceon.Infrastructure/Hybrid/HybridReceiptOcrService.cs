using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.ReceiptScan;
using Traceon.Infrastructure.OpenAI;

namespace Traceon.Infrastructure.Hybrid;

/// <summary>
/// Two-stage receipt OCR:
///   1. Azure Document Intelligence (prebuilt-layout) → accurate raw OCR text
///   2. OpenAI GPT-4.1 → semantic extraction (items, discounts, categories, etc.)
///
/// This avoids LLM hallucinating prices (DI reads exact printed text) while leveraging
/// the LLM's ability to understand receipt structure (discounts, headers, split lines).
/// </summary>
public sealed class HybridReceiptOcrService(
    DocumentIntelligenceClient diClient,
    HttpClient httpClient,
    IOptions<OpenAISettings> openAiOptions,
    ILogger<HybridReceiptOcrService> logger) : IReceiptOcrService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private const string SystemPrompt = """
        You are a receipt parser. You will receive the raw OCR text extracted from a receipt image.
        Extract structured data and return ONLY valid JSON.

        Rules:
        - The OCR text is accurate — use the EXACT numbers from the text, do NOT estimate or round
        - Skip category headers (lines with no price that group products like "DAIRY", "PRODUCE", etc.)
        - Quantity defaults to 1 if not shown
        - If totalPrice is missing, calculate from (quantity × unitPrice) - discount
        - Discounts can be item-level (e.g. "2 for $5") or separate lines (e.g. "20% off -$1.00")
        - Discount lines may appear indented below the item, on the same line in parentheses, or as a separate line with a negative value
        - For separate discount lines, attach the discount to the preceding item rather than creating a new item
        - discount is the absolute amount subtracted from that item (always positive or null)
        - totalPrice is the final price AFTER discount for that item
        - totalDiscount is ONLY a receipt-wide / receipt-scoped discount applied AFTER the subtotal (e.g. "10% off total", "$5 loyalty credit", coupon on the whole order). DO NOT sum per-item discounts into totalDiscount. If there is no explicit receipt-wide discount line, return null.
        - Use null for any value you cannot determine
        - Dates must be in ISO 8601 format (yyyy-MM-ddTHH:mm:ss)
        - All monetary values are numbers (no currency symbols)

        JSON schema:
        {
          "merchantName": "string or null",
          "transactionDate": "ISO 8601 string or null",
          "subtotal": number or null,
          "tax": number or null,
          "totalDiscount": number or null,
          "total": number or null,
          "items": [
            {
              "description": "string",
              "quantity": number or null,
              "unitPrice": number or null,
              "discount": number or null,
              "totalPrice": number or null
            }
          ]
        }
        """;

    public async Task<Result<ReceiptScanResponse>> ScanReceiptAsync(
        Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Stage 1: OCR via Document Intelligence (prebuilt-layout for raw text)
            var ocrText = await ExtractOcrTextAsync(imageStream, fileName, cancellationToken);
            if (ocrText is null)
                return Result<ReceiptScanResponse>.Failure("Could not extract text from receipt.", ResultErrorType.Validation);

            // Stage 2: Structured extraction via LLM
            return await ExtractStructuredDataAsync(ocrText, fileName, cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Document Intelligence request failed for {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure(
                $"OCR failed: {ex.Message}", ResultErrorType.Validation);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "OpenAI request failed for {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure(
                $"AI service unavailable: {ex.Message}", ResultErrorType.Validation);
        }
    }

    // ── Stage 1: Document Intelligence OCR ──────────────────────

    private async Task<string?> ExtractOcrTextAsync(
        Stream imageStream, string fileName, CancellationToken cancellationToken)
    {
        var binaryData = await BinaryData.FromStreamAsync(imageStream, cancellationToken);

        var operation = await diClient.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-layout",
            binaryData,
            cancellationToken);

        var result = operation.Value;
        var content = result.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            logger.LogWarning("Document Intelligence returned empty content for {FileName}.", fileName);
            return null;
        }

        // Also append table data if present (some receipts are tabular)
        var sb = new StringBuilder(content);
        if (result.Tables is { Count: > 0 })
        {
            sb.AppendLine();
            sb.AppendLine("--- Tables ---");
            foreach (var table in result.Tables)
            {
                foreach (var cell in table.Cells)
                {
                    sb.Append($"[R{cell.RowIndex}C{cell.ColumnIndex}] {cell.Content}  ");
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // ── Stage 2: LLM structured extraction ──────────────────────

    private async Task<Result<ReceiptScanResponse>> ExtractStructuredDataAsync(
        string ocrText, string fileName, CancellationToken cancellationToken)
    {
        var settings = openAiOptions.Value;

        var request = new
        {
            model = settings.Model,
            max_tokens = settings.MaxTokens,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new
                {
                    role = "user",
                    content = $"Extract all line items and receipt data from this OCR text:\n\n{ocrText}"
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        httpRequest.Content = JsonContent.Create(request);

        using var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("OpenAI API returned {Status}: {Body}", httpResponse.StatusCode, errorBody);
            return Result<ReceiptScanResponse>.Failure(
                $"AI service returned {httpResponse.StatusCode}.", ResultErrorType.Validation);
        }

        var chatResponse = await httpResponse.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken);
        var content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;

        if (string.IsNullOrWhiteSpace(content))
        {
            logger.LogWarning("OpenAI returned empty content for {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure("AI returned no content.", ResultErrorType.Validation);
        }

        var parsed = JsonSerializer.Deserialize<LlmReceiptResult>(content, JsonOptions);

        if (parsed is null)
        {
            logger.LogWarning("Failed to deserialize LLM response for {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure("Failed to parse AI response.", ResultErrorType.Validation);
        }

        var items = (parsed.Items ?? []).Select(i =>
        {
            var totalPrice = i.TotalPrice ?? (i.Quantity.HasValue && i.UnitPrice.HasValue
                ? i.Quantity.Value * i.UnitPrice.Value - (i.Discount ?? 0)
                : i.UnitPrice.HasValue ? i.UnitPrice.Value - (i.Discount ?? 0) : null);

            return new ReceiptScanLineItemResponse
            {
                Description = i.Description ?? "—",
                Quantity = i.Quantity ?? 1,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalPrice = totalPrice
            };
        }).ToList();

        logger.LogInformation(
            "Hybrid receipt scan: Merchant={Merchant}, Items={ItemCount}, Total={Total}, Model={Model}",
            parsed.MerchantName, items.Count, parsed.Total, settings.Model);

        return Result<ReceiptScanResponse>.Success(new ReceiptScanResponse
        {
            MerchantName = parsed.MerchantName,
            TransactionDate = parsed.TransactionDate,
            Items = items,
            Subtotal = parsed.Subtotal,
            Tax = parsed.Tax,
            TotalDiscount = parsed.TotalDiscount,
            Total = parsed.Total,
            Confidence = 0.9
        });
    }

    // ── Internal DTOs ───────────────────────────────────────────

    private sealed class ChatCompletionResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        public MessageContent? Message { get; set; }
    }

    private sealed class MessageContent
    {
        public string? Content { get; set; }
    }

    private sealed class LlmReceiptResult
    {
        public string? MerchantName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? TotalDiscount { get; set; }
        public decimal? Total { get; set; }
        public List<LlmLineItem>? Items { get; set; }
    }

    private sealed class LlmLineItem
    {
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
