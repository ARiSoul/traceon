using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.ReceiptScan;
using Traceon.Infrastructure.ReceiptScan;

namespace Traceon.Infrastructure.OpenAI;

public sealed class LlmReceiptOcrService(
    HttpClient httpClient,
    IOptions<OpenAISettings> options,
    ILogger<LlmReceiptOcrService> logger) : IReceiptOcrService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private const string SystemPrompt = """
        You are a receipt parser. Extract structured data from the receipt image.
        Return ONLY valid JSON with no markdown, no code fences, no explanation.

        Rules:
        - COPY the exact numbers printed on the receipt. NEVER calculate, estimate or round any value; all arithmetic is done afterwards by code.
        - Skip category headers (lines with no price that group products)
        - Quantity defaults to 1 if not shown (for weighed items it is the weight, e.g. 1,736)
        - unitPrice is the price per unit / per kg exactly as printed
        - lineAmount is the amount printed on the item's own line, copied exactly. Example: a row "1,144 x 12,99 = 14,86" → lineAmount = 14.86. Use null if the item line has no amount of its own.
        - Discounts can be item-level (e.g. "2 for $5") or separate lines (e.g. "20% off -$1.00"). Sometimes it may appear in an idented line below the item it applies to, or on the same line in parentheses. Always try to associate discounts with the correct item.
        - For separate discount lines, attach the discount to the preceding item rather than creating a new item
        - discount is the absolute amount printed for that item's discount (always positive or null), copied exactly
        - totalDiscount is ONLY a receipt-wide discount applied after the subtotal; do not sum per-item discounts into it. Otherwise null.
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
              "lineAmount": number or null,
              "discount": number or null
            }
          ]
        }
        """;

    public async Task<Result<ReceiptScanResponse>> ScanReceiptAsync(
        Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        try
        {
            // Read image and convert to base64
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms, cancellationToken);
            var base64 = Convert.ToBase64String(ms.ToArray());

            var mediaType = GetMediaType(fileName);

            // Build the OpenAI Chat Completions request
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
                        content = new object[]
                        {
                            new { type = "text", text = "Extract all line items and receipt data from this image." },
                            new
                            {
                                type = "image_url",
                                image_url = new { url = $"data:{mediaType};base64,{base64}", detail = "high" }
                            }
                        }
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
                logger.LogWarning("OpenAI returned empty content for file {FileName}.", fileName);
                return Result<ReceiptScanResponse>.Failure("AI returned no content.", ResultErrorType.Validation);
            }

            var parsed = JsonSerializer.Deserialize<LlmReceiptResult>(content, JsonOptions);

            if (parsed is null)
            {
                logger.LogWarning("Failed to deserialize LLM response for file {FileName}.", fileName);
                return Result<ReceiptScanResponse>.Failure("Failed to parse AI response.", ResultErrorType.Validation);
            }

            var items = (parsed.Items ?? []).Select(i => new ReceiptScanLineItemResponse
            {
                Description = i.Description ?? "—",
                Quantity = i.Quantity ?? 1,
                UnitPrice = i.UnitPrice,
                TotalPrice = ReceiptLineMath.ComputeLineTotal(i.Quantity, i.UnitPrice, i.Discount, i.LineAmount, i.TotalPrice),
                Discount = i.Discount
            }).ToList();

            var mismatch = ReceiptLineMath.TotalMismatch(items.Select(i => i.TotalPrice), parsed.TotalDiscount, parsed.Total);
            if (mismatch is { } diff && Math.Abs(diff) > 0.05m)
                logger.LogWarning(
                    "LLM receipt scan: line totals differ from receipt total by {Difference} for {FileName} (Total={Total}).",
                    diff, fileName, parsed.Total);

            logger.LogInformation(
                "LLM receipt scan: Merchant={Merchant}, Items={ItemCount}, Total={Total}, Model={Model}",
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
                Confidence = 0.95 // LLMs don't provide a confidence score; use a sensible default
            });
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON parsing failed for LLM receipt scan of {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure("Failed to parse AI response.", ResultErrorType.Validation);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP request to OpenAI failed for {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure(
                $"AI service unavailable: {ex.Message}", ResultErrorType.Validation);
        }
    }

    private static string GetMediaType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    // ── Internal DTOs for JSON deserialization ──

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
        public decimal? LineAmount { get; set; }
        public decimal? Discount { get; set; }
        // No longer requested; only used as a fallback if the model still returns it
        public decimal? TotalPrice { get; set; }
    }
}
