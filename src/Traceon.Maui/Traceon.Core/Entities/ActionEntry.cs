namespace Arisoul.Traceon.Maui.Core.Entities;

public class ActionEntry
{
    public Guid Id { get; set; }
    public Guid TrackedActionId { get; set; }

    public DateTime Timestamp { get; set; }
    public TimeSpan? Duration { get; set; }
    public double? Quantity { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }

    public Dictionary<string, string>? CustomValues { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}