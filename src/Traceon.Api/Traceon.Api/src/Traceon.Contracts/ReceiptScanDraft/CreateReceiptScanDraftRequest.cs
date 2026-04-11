namespace Traceon.Contracts.ReceiptScanDraft;

public sealed record CreateReceiptScanDraftRequest(
    Guid SelectedActionId,
    string SelectedActionName,
    string SerializedState,
    string? MerchantName = null,
    DateTime? TransactionDate = null,
    decimal? Total = null,
    int CurrentStep = 0);
