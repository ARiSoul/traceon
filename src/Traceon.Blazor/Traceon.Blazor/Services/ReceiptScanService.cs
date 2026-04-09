namespace Traceon.Blazor.Services;

/// <summary>
/// Client-side service for receipt OCR scanning.
/// Currently uses a mock implementation; will be replaced with Azure Document Intelligence calls.
/// </summary>
public sealed class ReceiptScanService
{
    /// <summary>
    /// Simulates sending an image to OCR and receiving structured receipt data.
    /// </summary>
    public async Task<ReceiptScanResult> ScanAsync(Stream imageStream, string fileName)
    {
        // Simulate network/processing delay
        await Task.Delay(1500);

        // Return realistic mock data for a grocery receipt
        return new ReceiptScanResult
        {
            MerchantName = "Lidl",
            TransactionDate = DateTime.Today,
            Items =
            [
                new ReceiptLineItem { Description = "Organic Whole Milk 1L", Quantity = 2, UnitPrice = 0.99m, TotalPrice = 1.98m },
                new ReceiptLineItem { Description = "Sourdough Bread 500g", Quantity = 1, UnitPrice = 1.49m, TotalPrice = 1.49m },
                new ReceiptLineItem { Description = "Free Range Eggs x12", Quantity = 1, UnitPrice = 2.79m, TotalPrice = 2.79m },
                new ReceiptLineItem { Description = "Bananas 1kg", Quantity = 1, UnitPrice = 1.19m, TotalPrice = 1.19m },
                new ReceiptLineItem { Description = "Cherry Tomatoes 250g", Quantity = 2, UnitPrice = 1.29m, TotalPrice = 2.58m },
                new ReceiptLineItem { Description = "Chicken Breast 500g", Quantity = 1, UnitPrice = 3.99m, TotalPrice = 3.99m },
                new ReceiptLineItem { Description = "Olive Oil Extra Virgin 750ml", Quantity = 1, UnitPrice = 4.49m, TotalPrice = 4.49m },
                new ReceiptLineItem { Description = "Cheddar Cheese 200g", Quantity = 1, UnitPrice = 1.89m, TotalPrice = 1.89m },
                new ReceiptLineItem { Description = "Plastic Bag", Quantity = 1, UnitPrice = 0.10m, TotalPrice = 0.10m },
                new ReceiptLineItem { Description = "Dishwasher Tablets x30", Quantity = 1, UnitPrice = 3.49m, TotalPrice = 3.49m },
            ],
            Subtotal = 23.99m,
            Tax = 1.92m,
            Total = 25.91m,
            Confidence = 0.94
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
    public decimal? TotalPrice { get; set; }
}
