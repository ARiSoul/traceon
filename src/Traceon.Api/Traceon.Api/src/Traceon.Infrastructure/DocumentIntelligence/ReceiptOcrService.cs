using System.Globalization;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Logging;
using Traceon.Application.Common;
using Traceon.Application.Interfaces;
using Traceon.Contracts.ReceiptScan;

namespace Traceon.Infrastructure.DocumentIntelligence;

public sealed class ReceiptOcrService(
    DocumentIntelligenceClient client,
    ILogger<ReceiptOcrService> logger) : IReceiptOcrService
{
    public async Task<Result<ReceiptScanResponse>> ScanReceiptAsync(
        Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var binaryData = await BinaryData.FromStreamAsync(imageStream, cancellationToken);

            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-receipt",
                binaryData,
                cancellationToken);

            var result = operation.Value;

            if (result.Documents is null || result.Documents.Count == 0)
            {
                logger.LogWarning("Document Intelligence returned no documents for file {FileName}.", fileName);
                return Result<ReceiptScanResponse>.Failure("No receipt detected in the image.", ResultErrorType.Validation);
            }

            var receipt = result.Documents[0];
            var fields = receipt.Fields;

            var merchantName = GetStringField(fields, "MerchantName");
            var transactionDate = GetDateField(fields, "TransactionDate");
            var subtotal = GetCurrencyAmount(fields, "Subtotal");
            var tax = GetCurrencyAmount(fields, "TotalTax");
            var totalDiscount = GetCurrencyAmount(fields, "TotalDiscount");
            var total = GetCurrencyAmount(fields, "Total");

            var items = new List<ReceiptScanLineItemResponse>();

            if (fields.TryGetValue("Items", out var itemsField) && itemsField.ValueList is { Count: > 0 })
            {
                foreach (var itemDoc in itemsField.ValueList)
                {
                    if (itemDoc.ValueDictionary is null) continue;

                    var itemFields = itemDoc.ValueDictionary;
                    var description = GetStringField(itemFields, "Description") ?? "—";
                    var quantity = GetNumberField(itemFields, "Quantity");
                    var unitPrice = GetCurrencyAmount(itemFields, "Price");
                    var discount = GetCurrencyAmount(itemFields, "Discount");
                    var totalPrice = GetCurrencyAmount(itemFields, "TotalPrice");

                    // If total price is missing, calculate from qty × unit
                    totalPrice ??= quantity.HasValue && unitPrice.HasValue
                        ? quantity.Value * unitPrice.Value
                        : unitPrice;

                    // If quantity is missing but we have both prices, infer it
                    if (quantity is null && totalPrice.HasValue && unitPrice is > 0)
                        quantity = totalPrice.Value / unitPrice.Value;

                    // Default quantity to 1 if still null
                    quantity ??= 1;

                    items.Add(new ReceiptScanLineItemResponse
                    {
                        Description = description,
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Discount = discount,
                        TotalPrice = totalPrice
                    });
                }
            }

            var confidence = receipt.Confidence;

            logger.LogInformation(
                "Receipt scanned: Merchant={Merchant}, Items={ItemCount}, Total={Total}, Confidence={Confidence:P0}",
                merchantName, items.Count, total, confidence);

            return Result<ReceiptScanResponse>.Success(new ReceiptScanResponse
            {
                MerchantName = merchantName,
                TransactionDate = transactionDate,
                Items = items,
                Subtotal = subtotal,
                Tax = tax,
                TotalDiscount = totalDiscount,
                Total = total,
                Confidence = confidence
            });
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Document Intelligence request failed for file {FileName}.", fileName);
            return Result<ReceiptScanResponse>.Failure(
                $"Receipt scanning failed: {ex.Message}", ResultErrorType.Validation);
        }
    }

    private static string? GetStringField(IReadOnlyDictionary<string, DocumentField> fields, string key) =>
        fields.TryGetValue(key, out var field) ? field.ValueString : null;

    private static DateTime? GetDateField(IReadOnlyDictionary<string, DocumentField> fields, string key) =>
        fields.TryGetValue(key, out var field) ? field.ValueDate?.DateTime : null;

    private static decimal? GetCurrencyAmount(IReadOnlyDictionary<string, DocumentField> fields, string key)
    {
        if (!fields.TryGetValue(key, out var field)) return null;

        if (field.ValueCurrency is { } currency)
            return Convert.ToDecimal(currency.Amount, CultureInfo.InvariantCulture);

        if (field.ValueDouble is { } number)
            return Convert.ToDecimal(number, CultureInfo.InvariantCulture);

        return null;
    }

    private static decimal? GetNumberField(IReadOnlyDictionary<string, DocumentField> fields, string key)
    {
        if (!fields.TryGetValue(key, out var field)) return null;

        if (field.ValueDouble is { } number)
            return Convert.ToDecimal(number, CultureInfo.InvariantCulture);

        return null;
    }
}
