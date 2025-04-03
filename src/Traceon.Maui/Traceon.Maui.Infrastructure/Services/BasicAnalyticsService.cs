using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Infrastructure.Services;

public class BasicAnalyticsService(ITrackedActionRepository actionRespository) 
    : IAnalyticsService
{
    private readonly ITrackedActionRepository _actionRepository = actionRespository;

    public async Task<AnalyticsSummary> GetSummaryAsync(Guid trackedActionId)
    {
        var entries = (await _actionRepository.GetActionEntriesAsync(trackedActionId)).ToList();

        if (entries.Count == 0)
            return new AnalyticsSummary();

        return new AnalyticsSummary
        {
            TotalEntries = entries.Count,
            AverageDuration = TimeSpan.FromSeconds(entries.Select(e => e.Duration!.TotalSeconds).DefaultIfEmpty(0).Average()),
            AverageQuantity = entries.Select(e => e.Quantity!).DefaultIfEmpty(0).Average(),
            AverageCost = entries.Select(e => e.Cost).DefaultIfEmpty(0).Average(),
            LastEntryDate = entries.Max(e => e.Timestamp)
        };
    }

    public async Task<IEnumerable<TrendPoint>> GetTrendAsync(Guid trackedActionId)
    {
        var entries = await _actionRepository.GetActionEntriesAsync(trackedActionId);

        var grouped = entries
            .GroupBy(e => new DateTime(e.Timestamp!.Year, e.Timestamp.Month, 1))
            .Select(g => new TrendPoint
            {
                PeriodStart = g.Key,
                AverageDuration = TimeSpan.FromSeconds(g.Select(e => e.Duration!.TotalSeconds).DefaultIfEmpty(0).Average()),
                AverageQuantity = g.Select(e => e.Quantity).DefaultIfEmpty(0).Average(),
                AverageCost = g.Select(e => e.Cost).DefaultIfEmpty(0).Average(),
            });

        return grouped.OrderBy(g => g.PeriodStart);
    }
}