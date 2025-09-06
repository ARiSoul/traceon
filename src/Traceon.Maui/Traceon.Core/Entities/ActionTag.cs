using Arisoul.Traceon.Maui.Core.Entities;

public class ActionTag
    : BaseActionChildEntity
{
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
