using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Infrastructure.Services;

public class BasicAnalyticsService(ITrackedActionRepository actionRespository) 
    : IAnalyticsService
{
    private readonly ITrackedActionRepository _actionRepository = actionRespository;

    public async Task<AnalyticsSummary> GetSummaryAsync(Guid trackedActionId)
    {
        var entries = (await _actionRepository.GetActionEntriesAsync(trackedActionId)).ToList();

        if (!entries.Any())
            return new AnalyticsSummary();

        return new AnalyticsSummary
        {
            TotalEntries = entries.Count,
            AverageDuration = TimeSpan.FromSeconds(entries.Where(e => e.Duration != null).Select(e => e.Duration!.Value.TotalSeconds).DefaultIfEmpty(0).Average()),
            AverageQuantity = entries.Where(e => e.Quantity != null).Select(e => e.Quantity!.Value).DefaultIfEmpty(0).Average(),
            AverageCost = entries.Where(e => e.Cost != null).Select(e => e.Cost!.Value).DefaultIfEmpty(0).Average(),
            LastEntryDate = entries.Max(e => e.Timestamp)
        };
    }

    public async Task<IEnumerable<TrendPoint>> GetTrendAsync(Guid trackedActionId)
    {
        var entries = await _actionRepository.GetActionEntriesAsync(trackedActionId);

        var grouped = entries
            .GroupBy(e => new DateTime(e.Timestamp!.Value.Year, e.Timestamp.Value.Month, 1))
            .Select(g => new TrendPoint
            {
                PeriodStart = g.Key,
                AverageDuration = TimeSpan.FromSeconds(g.Where(e => e.Duration != null).Select(e => e.Duration!.Value.TotalSeconds).DefaultIfEmpty(0).Average()),
                AverageQuantity = g.Where(e => e.Quantity != null).Select(e => e.Quantity!.Value).DefaultIfEmpty(0).Average(),
                AverageCost = g.Where(e => e.Cost != null).Select(e => e.Cost!.Value).DefaultIfEmpty(0).Average(),
            });

        return grouped.OrderBy(g => g.PeriodStart);
    }
}