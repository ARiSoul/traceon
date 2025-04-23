namespace Arisoul.Traceon.Maui.Core.Entities;

public class Audit
{
    public Guid Id { get; set; }
    public string Entity { get; set; } = string.Empty; // e.g. "TrackedAction"
    public string Operation { get; set; } = string.Empty; // create, update, delete
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string? User { get; set; }
}

