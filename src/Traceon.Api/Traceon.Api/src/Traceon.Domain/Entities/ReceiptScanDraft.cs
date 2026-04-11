namespace Traceon.Domain.Entities;

/// <summary>
/// Persists an in-progress receipt scan so the user can resume later.
/// Deleted automatically on successful import.
/// </summary>
public sealed class ReceiptScanDraft : OwnedEntity
{
    public string? MerchantName { get; private set; }
    public DateTime? TransactionDate { get; private set; }
    public decimal? Total { get; private set; }
    public int CurrentStep { get; private set; }
    public Guid SelectedActionId { get; private set; }
    public string SelectedActionName { get; private set; } = string.Empty;

    /// <summary>JSON blob containing the full wizard state (scan result, items, mappings, etc.).</summary>
    public string SerializedState { get; private set; } = string.Empty;

    private ReceiptScanDraft() { }

    public static ReceiptScanDraft Create(
        string userId,
        Guid selectedActionId,
        string selectedActionName,
        string serializedState,
        string? merchantName = null,
        DateTime? transactionDate = null,
        decimal? total = null,
        int currentStep = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(serializedState);

        var draft = new ReceiptScanDraft
        {
            SelectedActionId = selectedActionId,
            SelectedActionName = selectedActionName,
            SerializedState = serializedState,
            MerchantName = merchantName,
            TransactionDate = transactionDate,
            Total = total,
            CurrentStep = currentStep
        };
        draft.SetOwner(userId);
        return draft;
    }

    public void Update(
        string serializedState,
        string? merchantName = null,
        DateTime? transactionDate = null,
        decimal? total = null,
        int currentStep = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serializedState);

        SerializedState = serializedState;
        MerchantName = merchantName;
        TransactionDate = transactionDate;
        Total = total;
        CurrentStep = currentStep;
        MarkUpdated();
    }
}
