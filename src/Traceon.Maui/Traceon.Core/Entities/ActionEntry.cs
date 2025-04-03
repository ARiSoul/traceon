namespace Arisoul.Traceon.Maui.Core.Entities;

public class ActionEntry
{
    public Guid Id { get; set; }
    public Guid TrackedActionId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    public double Quantity { get; set; } = 0;
    public decimal Cost { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;

    public Dictionary<string, string>? CustomValues { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}