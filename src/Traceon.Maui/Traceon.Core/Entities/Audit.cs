namespace Arisoul.Traceon.Maui.Core.Entities;

public class Audit
    : BaseEntityWithId
{
    public string Entity { get; set; } = string.Empty; // e.g. "TrackedAction"
    public string EntityId { get; set; } = string.Empty; // e.g. "123e4567-e89b-12d3-a456-426614174000"
    public string Operation { get; set; } = string.Empty; // create, update, delete
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string? User { get; set; }
}

