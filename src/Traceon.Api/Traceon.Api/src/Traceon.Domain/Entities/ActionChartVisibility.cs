namespace Traceon.Domain.Entities;

public sealed class ActionChartVisibility : OwnedEntity
{
    public Guid TrackedActionId { get; private set; }

    /// <summary>Pipe-separated list of chart keys hidden by the user.</summary>
    public string HiddenKeys { get; private set; } = string.Empty;

    private ActionChartVisibility() { }

    public static ActionChartVisibility Create(string userId, Guid trackedActionId, string hiddenKeys)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        var entity = new ActionChartVisibility
        {
            TrackedActionId = trackedActionId,
            HiddenKeys = hiddenKeys ?? string.Empty
        };
        entity.SetOwner(userId);
        return entity;
    }

    public void SetHiddenKeys(string hiddenKeys)
    {
        HiddenKeys = hiddenKeys ?? string.Empty;
        MarkUpdated();
    }
}
