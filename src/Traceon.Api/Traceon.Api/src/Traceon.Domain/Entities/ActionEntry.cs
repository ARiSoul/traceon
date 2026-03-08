namespace Traceon.Domain.Entities;

public sealed class ActionEntry : Entity
{
    public Guid TrackedActionId { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private readonly List<ActionEntryField> _fields = [];
    public IReadOnlyCollection<ActionEntryField> Fields => _fields.AsReadOnly();

    private ActionEntry(Guid trackedActionId, DateTime occurredAtUtc)
    {
        TrackedActionId = trackedActionId;
        OccurredAtUtc = occurredAtUtc;
    }

    public static ActionEntry Create(Guid trackedActionId, DateTime occurredAtUtc)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        return new ActionEntry(trackedActionId, occurredAtUtc);
    }

    public void Update(DateTime occurredAtUtc)
    {
        OccurredAtUtc = occurredAtUtc;
        MarkUpdated();
    }

    public void SetFieldValue(Guid actionFieldId, string? value)
    {
        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        var existing = _fields.FirstOrDefault(f => f.ActionFieldId == actionFieldId);

        if (existing is not null)
        {
            existing.UpdateValue(value);
        }
        else
        {
            _fields.Add(ActionEntryField.Create(Id, actionFieldId, value));
        }
    }

    public void ClearFields() => _fields.Clear();
}
