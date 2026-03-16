namespace Traceon.Domain.Entities;

public sealed class TrackedActionTag
{
    public Guid TrackedActionId { get; private set; }
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;

    private TrackedActionTag(Guid trackedActionId, Guid tagId)
    {
        TrackedActionId = trackedActionId;
        TagId = tagId;
    }

    public static TrackedActionTag Create(Guid trackedActionId, Guid tagId)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        if (tagId == Guid.Empty)
            throw new ArgumentException("Tag ID is required.", nameof(tagId));

        return new TrackedActionTag(trackedActionId, tagId);
    }
}
