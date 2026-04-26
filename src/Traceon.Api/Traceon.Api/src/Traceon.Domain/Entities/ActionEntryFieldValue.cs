namespace Traceon.Domain.Entities;

public sealed class ActionEntryFieldValue : Entity
{
    public Guid ActionEntryFieldId { get; set; }
    public string Value { get; set; } = string.Empty;
    public int Order { get; set; }

    public static ActionEntryFieldValue Create(Guid actionEntryFieldId, string value, int order)
    {
        if (actionEntryFieldId == Guid.Empty)
            throw new ArgumentException("Action entry field ID is required.", nameof(actionEntryFieldId));
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new ActionEntryFieldValue
        {
            ActionEntryFieldId = actionEntryFieldId,
            Value = value.Trim(),
            Order = order
        };
    }
}
