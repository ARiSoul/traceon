namespace Traceon.Infrastructure.Persistence;

public sealed class PurgeSettings
{
    public int IntervalHours { get; set; } = 24;
    public int DefaultRetentionDays { get; set; } = 180;
}
