namespace Traceon.Domain.Entities;

public sealed class EntryTemplateField : Entity
{
    public Guid EntryTemplateId { get; set; }
    public Guid ActionFieldId { get; set; }
    public string? Value { get; set; }

    public static EntryTemplateField Create(Guid entryTemplateId, Guid actionFieldId, string? value)
    {
        if (entryTemplateId == Guid.Empty)
            throw new ArgumentException("Entry template ID is required.", nameof(entryTemplateId));

        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        return new EntryTemplateField
        {
            EntryTemplateId = entryTemplateId,
            ActionFieldId = actionFieldId,
            Value = value?.Trim()
        };
    }

    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
        MarkUpdated();
    }
}
