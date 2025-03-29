namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsSummary> GetSummaryAsync(Guid trackedActionId);
    Task<IEnumerable<TrendPoint>> GetTrendAsync(Guid trackedActionId);
}

public class AnalyticsSummary
{
    public int TotalEntries { get; set; }
    public TimeSpan? AverageDuration { get; set; }
    public double? AverageQuantity { get; set; }
    public decimal? AverageCost { get; set; }
    public DateTime? LastEntryDate { get; set; }
}

public class TrendPoint
{
    public DateTime PeriodStart { get; set; }
    public TimeSpan? AverageDuration { get; set; }
    public double? AverageQuantity { get; set; }
    public decimal? AverageCost { get; set; }
}
