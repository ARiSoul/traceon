namespace Traceon.Domain.Entities;

public sealed class ActionEntryField : Entity
{
    public Guid ActionEntryId { get; set; }
    public Guid ActionFieldId { get; set; }

    private readonly List<ActionEntryFieldValue> _values = [];
    public IReadOnlyCollection<ActionEntryFieldValue> Values => _values.AsReadOnly();

    public static ActionEntryField Create(Guid actionEntryId, Guid actionFieldId)
    {
        if (actionEntryId == Guid.Empty)
            throw new ArgumentException("Action entry ID is required.", nameof(actionEntryId));

        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        return new ActionEntryField
        {
            ActionEntryId = actionEntryId,
            ActionFieldId = actionFieldId
        };
    }

    public void SetValues(IEnumerable<string>? values)
    {
        _values.Clear();

        if (values is null)
        {
            MarkUpdated();
            return;
        }

        var order = 0;
        foreach (var raw in values)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            _values.Add(ActionEntryFieldValue.Create(Id, raw.Trim(), order++));
        }

        MarkUpdated();
    }
}
