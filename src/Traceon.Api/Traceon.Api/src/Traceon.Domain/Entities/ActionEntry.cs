namespace Traceon.Domain.Entities;

public sealed class ActionEntry : Entity
{
    public Guid TrackedActionId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ReceiptImportBatchId { get; private set; }

    private readonly List<ActionEntryField> _fields = [];
    public IReadOnlyCollection<ActionEntryField> Fields => _fields.AsReadOnly();

    private ActionEntry(Guid trackedActionId, DateTime occurredAtUtc, string? notes, Guid? receiptImportBatchId)
    {
        TrackedActionId = trackedActionId;
        OccurredAtUtc = occurredAtUtc;
        Notes = notes;
        ReceiptImportBatchId = receiptImportBatchId;
    }

    public static ActionEntry Create(
        Guid trackedActionId,
        DateTime occurredAtUtc,
        string? notes = null,
        Guid? receiptImportBatchId = null)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        return new ActionEntry(trackedActionId, occurredAtUtc, notes?.Trim(), receiptImportBatchId);
    }

    public void Update(DateTime occurredAtUtc, string? notes = null)
    {
        OccurredAtUtc = occurredAtUtc;
        Notes = notes?.Trim();
        MarkUpdated();
    }

    public void SetFieldValues(Guid actionFieldId, IEnumerable<string>? values)
    {
        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        var existing = _fields.FirstOrDefault(f => f.ActionFieldId == actionFieldId);
        if (existing is not null)
        {
            existing.SetValues(values);
            return;
        }

        var slot = ActionEntryField.Create(Id, actionFieldId);
        slot.SetValues(values);
        _fields.Add(slot);
    }

    public void ClearFields() => _fields.Clear();
}
