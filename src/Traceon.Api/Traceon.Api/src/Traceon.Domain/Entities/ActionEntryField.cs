namespace Traceon.Domain.Entities;

public sealed class ActionEntryField : Entity
{
    public Guid ActionEntryId { get; set; }
    public Guid ActionFieldId { get; set; }
    public string? Value { get; set; }

    public static ActionEntryField Create(Guid actionEntryId, Guid actionFieldId, string? value)
    {
        if (actionEntryId == Guid.Empty)
            throw new ArgumentException("Action entry ID is required.", nameof(actionEntryId));

        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        return new ActionEntryField
        {
            ActionEntryId = actionEntryId, 
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
