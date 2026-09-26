namespace Traceon.Infrastructure.ReceiptScan;

/// <summary>
/// Deterministic line-total arithmetic for scanned receipts.
///
/// The LLM is only trusted to COPY numbers from the receipt (quantity, unit price, the amount
/// printed on the item line, discount). It is not trusted to do arithmetic: it regularly returned
/// totals off by cents or whole euros (e.g. 2 × 14.35 − 1.72 = 27.98 instead of 26.98).
/// </summary>
public static class ReceiptLineMath
{
    private const decimal Tolerance = 0.02m;

    /// <summary>
    /// Computes the final (after discount) total of a receipt line.
    /// Priority: printed line amount − discount → round(quantity × unitPrice) − discount
    /// → LLM total (legacy fallback) → unitPrice − discount.
    /// </summary>
    public static decimal? ComputeLineTotal(
        decimal? quantity, decimal? unitPrice, decimal? discount, decimal? lineAmount, decimal? llmTotal)
    {
        var disc = discount is > 0 ? discount.Value : 0m;
        decimal? expectedGross = quantity.HasValue && unitPrice.HasValue
            ? Round(quantity.Value * unitPrice.Value)
            : null;

        if (lineAmount is { } printed)
        {
            // Some receipts print the already-discounted amount on the item line; don't subtract twice.
            if (disc > 0 && expectedGross is { } g && Near(printed, g - disc) && !Near(printed, g))
                return Round(printed);

            return Round(printed - disc);
        }

        if (expectedGross is { } gross)
            return Round(gross - disc);

        if (llmTotal.HasValue)
            return Round(llmTotal.Value);

        return unitPrice.HasValue ? Round(unitPrice.Value - disc) : null;
    }

    /// <summary>
    /// Difference between the sum of the computed lines (minus any receipt-scoped discount) and
    /// the receipt's printed total, or null when there is no total to compare against.
    /// </summary>
    public static decimal? TotalMismatch(
        IReadOnlyCollection<(decimal? Total, decimal? Discount)> lines, decimal? totalDiscount, decimal? receiptTotal)
    {
        if (!receiptTotal.HasValue) return null;

        // The model often reports the sum of per-item discounts (e.g. Pingo Doce's "Poupança")
        // as totalDiscount; those are already subtracted in each line. Only the excess is
        // receipt-scoped — same rule as ComputeReceiptScopedDiscount in ReceiptScan.razor.
        var perItemDiscounts = lines.Sum(l => l.Discount is > 0 ? l.Discount.Value : 0);
        var receiptScoped = Math.Max((totalDiscount ?? 0) - perItemDiscounts, 0);

        var computed = lines.Sum(l => l.Total ?? 0) - receiptScoped;
        return Round(computed - receiptTotal.Value);
    }

    // Receipts round half away from zero (1.736 × 4.99 = 8.66264 → 8.66; x.xx5 → up)
    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static bool Near(decimal a, decimal b) => Math.Abs(a - b) <= Tolerance;
}
