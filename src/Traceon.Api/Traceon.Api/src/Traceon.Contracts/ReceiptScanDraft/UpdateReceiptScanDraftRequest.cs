namespace Traceon.Contracts.ReceiptScanDraft;

public sealed record UpdateReceiptScanDraftRequest(
    string SerializedState,
    string? MerchantName = null,
    DateTime? TransactionDate = null,
    decimal? Total = null,
    int CurrentStep = 0);
