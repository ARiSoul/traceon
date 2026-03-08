namespace Traceon.Domain.Entities;

public sealed class ActionEntryField : Entity
{
    public Guid ActionEntryId { get; private set; }
    public Guid ActionFieldId { get; private set; }
    public string? Value { get; private set; }

    private ActionEntryField(Guid actionEntryId, Guid actionFieldId, string? value)
    {
        ActionEntryId = actionEntryId;
        ActionFieldId = actionFieldId;
        Value = value;
    }

    public static ActionEntryField Create(Guid actionEntryId, Guid actionFieldId, string? value)
    {
        if (actionEntryId == Guid.Empty)
            throw new ArgumentException("Action entry ID is required.", nameof(actionEntryId));

        if (actionFieldId == Guid.Empty)
            throw new ArgumentException("Action field ID is required.", nameof(actionFieldId));

        return new ActionEntryField(actionEntryId, actionFieldId, value?.Trim());
    }

    public void UpdateValue(string? value)
    {
        Value = value?.Trim();
        MarkUpdated();
    }
}
