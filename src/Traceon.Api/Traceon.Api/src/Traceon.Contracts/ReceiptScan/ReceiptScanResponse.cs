namespace Traceon.Contracts.ReceiptScan;

public sealed record ReceiptScanResponse
{
    public required string? MerchantName { get; init; }
    public required DateTime? TransactionDate { get; init; }
    public required List<ReceiptScanLineItemResponse> Items { get; init; }
    public required decimal? Subtotal { get; init; }
    public required decimal? Tax { get; init; }
    public required decimal? TotalDiscount { get; init; }
    public required decimal? Total { get; init; }
    public required double Confidence { get; init; }
}

public sealed record ReceiptScanLineItemResponse
{
    public required string Description { get; init; }
    public required decimal? Quantity { get; init; }
    public required decimal? UnitPrice { get; init; }
    public required decimal? Discount { get; init; }
    public required decimal? TotalPrice { get; init; }
}
