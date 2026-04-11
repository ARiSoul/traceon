using System.Net.Http.Json;
using Traceon.Contracts.ReceiptScan;

namespace Traceon.Blazor.Services;

/// <summary>
/// Client-side service for receipt OCR scanning.
/// Sends the image to the API which calls Azure Document Intelligence.
/// </summary>
public sealed class ReceiptScanService(HttpClient http)
{
    /// <summary>
    /// Uploads an image to the API for OCR processing and returns structured receipt data.
    /// </summary>
    public async Task<ReceiptScanResult> ScanAsync(Stream imageStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(imageStream);
        content.Add(streamContent, "file", fileName);

        var response = await http.PostAsync("/api/receipt-scan", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Receipt scan failed ({response.StatusCode}): {error}");
        }

        var scanResponse = await response.Content.ReadFromJsonAsync<ReceiptScanResponse>()
            ?? throw new InvalidOperationException("Empty response from receipt scan API.");

        return new ReceiptScanResult
        {
            MerchantName = scanResponse.MerchantName,
            TransactionDate = scanResponse.TransactionDate,
            Subtotal = scanResponse.Subtotal,
            Tax = scanResponse.Tax,
            TotalDiscount = scanResponse.TotalDiscount,
            Total = scanResponse.Total,
            Confidence = scanResponse.Confidence,
            Items = scanResponse.Items.Select(i => new ReceiptLineItem
            {
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}

/// <summary>Result from OCR receipt scanning.</summary>
public sealed class ReceiptScanResult
{
    public string? MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public List<ReceiptLineItem> Items { get; set; } = [];
    public decimal? Subtotal { get; set; }
    public decimal? Tax { get; set; }
    public decimal? TotalDiscount { get; set; }
    public decimal? Total { get; set; }
    /// <summary>Overall OCR confidence (0.0–1.0).</summary>
    public double Confidence { get; set; }
}

/// <summary>A single line item extracted from a receipt.</summary>
public sealed class ReceiptLineItem
{
    public string Description { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalPrice { get; set; }
}
