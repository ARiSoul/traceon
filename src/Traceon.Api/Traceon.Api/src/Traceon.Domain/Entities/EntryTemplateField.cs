namespace Traceon.Domain.Entities;

public sealed class EntryTemplateField : Entity
{
    public Guid EntryTemplateId { get; set; }
    public Guid ActionFieldId { get; set; }

    private readonly List<EntryTemplateFieldValue> _values = [];
    public IReadOnlyCollection<EntryTemplateFieldValue> Values => _values.AsReadOnly();

    public static EntryTemplateField Create(Guid entryTemplateId, Guid actionFieldId)
    {
        if (entryTemplateId == Guid.Empty)
            throw new ArgumentException("Entry template ID is required.", nameof(entryTemplateId));

        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        return new EntryTemplateField
        {
            EntryTemplateId = entryTemplateId,
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
            _values.Add(EntryTemplateFieldValue.Create(Id, raw.Trim(), order++));
        }

        MarkUpdated();
    }
}
