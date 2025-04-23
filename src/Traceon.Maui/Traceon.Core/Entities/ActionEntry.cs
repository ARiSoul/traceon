namespace Arisoul.Traceon.Maui.Core.Entities;

public class ActionEntry
{
    public Guid Id { get; set; }
    public Guid ActionId { get; set; }
    public TrackedAction Action { get; set; } = null!;

    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> FieldValues { get; set; } = [];
}