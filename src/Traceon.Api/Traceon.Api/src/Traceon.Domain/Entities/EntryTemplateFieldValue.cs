namespace Traceon.Domain.Entities;

public sealed class EntryTemplateFieldValue : Entity
{
    public Guid EntryTemplateFieldId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int Order { get; set; }

    public static EntryTemplateFieldValue Create(Guid entryTemplateFieldId, string value, int order)
    {
        if (entryTemplateFieldId == Guid.Empty)
            throw new ArgumentException("Entry template field ID is required.", nameof(entryTemplateFieldId));
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new EntryTemplateFieldValue
        {
            EntryTemplateFieldId = entryTemplateFieldId,
            Value = value.Trim(),
            Order = order
        };
    }
}
