namespace Arisoul.Traceon.Maui.Core.Entities;

public class ActionEntry 
    : BaseEntityWithId
{
    public Guid ActionId { get; set; }
    public TrackedAction Action { get; set; } = null!;

    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> FieldValues { get; set; } = [];
}