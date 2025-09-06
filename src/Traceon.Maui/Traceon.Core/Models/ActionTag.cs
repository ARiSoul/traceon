namespace Arisoul.Traceon.Maui.Core.Models;

public class ActionTag
    : Entities.BaseActionChildEntity
{
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
