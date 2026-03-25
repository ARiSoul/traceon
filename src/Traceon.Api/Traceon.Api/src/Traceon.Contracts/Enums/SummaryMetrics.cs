namespace Traceon.Contracts.Enums;

[Flags]
public enum SummaryMetrics
{
    None = 0,
    Min = 1,
    Max = 2,
    Avg = 4,
    Sum = 8,
    Count = 16,
    Latest = 32,
    All = Min | Max | Avg | Sum | Count | Latest
}
