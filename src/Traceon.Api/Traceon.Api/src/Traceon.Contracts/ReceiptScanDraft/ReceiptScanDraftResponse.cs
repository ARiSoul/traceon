namespace Traceon.Contracts.ReceiptScanDraft;

public sealed class ReceiptScanDraftResponse
{
    public Guid Id { get; set; }
    public string? MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public decimal? Total { get; set; }
    public int CurrentStep { get; set; }
    public Guid SelectedActionId { get; set; }
    public string SelectedActionName { get; set; } = string.Empty;
    public string SerializedState { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
