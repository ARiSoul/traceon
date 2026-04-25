namespace Traceon.Domain.Entities;

/// <summary>
/// A reusable starting point for new entries on a TrackedAction.
/// Holds a snapshot of field values without an OccurredAtUtc — it's a template, not an entry.
/// </summary>
public sealed class EntryTemplate : Entity
{
    public Guid TrackedActionId { get; private set; }
    public string Name { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<EntryTemplateField> _fields = [];
    public IReadOnlyCollection<EntryTemplateField> Fields => _fields.AsReadOnly();

    private EntryTemplate(Guid trackedActionId, string name, string? notes)
    {
        TrackedActionId = trackedActionId;
        Name = name;
        Notes = notes;
    }

    public static EntryTemplate Create(Guid trackedActionId, string name, string? notes = null)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new EntryTemplate(trackedActionId, name.Trim(), notes?.Trim());
    }

    public void Update(string name, string? notes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Notes = notes?.Trim();
        MarkUpdated();
    }

    public void SetFieldValue(Guid actionFieldId, string? value)
    {
        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        var existing = _fields.FirstOrDefault(f => f.ActionFieldId == actionFieldId);

        if (existing is not null)
            existing.UpdateValue(value);
        else
            _fields.Add(EntryTemplateField.Create(Id, actionFieldId, value));
    }

    public void ClearFields() => _fields.Clear();
}
