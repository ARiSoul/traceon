using Arisoul.Traceon.Maui.Core.Entities;

public class ActionTag
{
    public Guid ActionId { get; set; }
    public TrackedAction Action { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
