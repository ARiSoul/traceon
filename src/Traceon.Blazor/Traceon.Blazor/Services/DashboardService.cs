using Traceon.Blazor.Components;
using Traceon.Contracts.ActionEntries;
using Traceon.Contracts.ActionFields;
using Traceon.Contracts.Enums;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Blazor.Services;

public sealed class DashboardService(
    TrackedActionService actionService,
    ActionEntryService entryService,
    ActionFieldService fieldService)
{
    public async Task<DashboardData> LoadAsync(DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var actions = await actionService.GetAllAsync();

        // Build OData filter for date range
        string? dateFilter = null;
        var filterParts = new List<string>();
        if (fromUtc.HasValue)
            filterParts.Add($"OccurredAtUtc ge {fromUtc.Value:yyyy-MM-ddTHH:mm:ssZ}");
        if (toUtc.HasValue)
            filterParts.Add($"OccurredAtUtc le {toUtc.Value:yyyy-MM-ddTHH:mm:ssZ}");
        if (filterParts.Count > 0)
            dateFilter = string.Join(" and ", filterParts);

        // Fetch entries with optional date filter
        var request = new DataGridRequest(1, 10_000, "OccurredAtUtc", true, null);
        var entryResult = await entryService.QueryAsync(request, null, dateFilter);
        var allEntries = entryResult.Items;

        // Fetch fields per action (only for actions that have fields)
        var fieldsByAction = new Dictionary<Guid, List<ActionFieldResponse>>();
        foreach (var action in actions.Where(a => a.FieldCount > 0))
        {
            var fields = await fieldService.GetByTrackedActionAsync(action.Id);
            fieldsByAction[action.Id] = fields;
        }

        var now = DateTime.UtcNow;
        var today = now.Date;

        var actionStats = new List<ActionStats>();
        foreach (var action in actions)
        {
            var entries = allEntries.Where(e => e.TrackedActionId == action.Id).ToList();
            fieldsByAction.TryGetValue(action.Id, out var fields);

            actionStats.Add(BuildActionStats(action, entries, fields ?? []));
        }

        return new DashboardData
        {
            TotalActions = actions.Count,
            TotalEntries = allEntries.Count,
            EntriesToday = allEntries.Count(e => e.OccurredAtUtc.Date == today),
            EntriesThisWeek = allEntries.Count(e => e.OccurredAtUtc >= today.AddDays(-(int)today.DayOfWeek)),
            EntriesThisMonth = allEntries.Count(e => e.OccurredAtUtc.Year == today.Year && e.OccurredAtUtc.Month == today.Month),
            OverallStreak = ComputeOverallStreak(allEntries, today),
            Actions = actionStats,
            GlobalDailyEntries = BuildDailyEntries(allEntries)
        };
    }

    public async Task<ActionDetailData?> LoadActionDetailAsync(Guid actionId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var action = await actionService.GetByIdAsync(actionId);
        if (action is null) return null;

        string? dateFilter = null;
        var filterParts = new List<string> { $"TrackedActionId eq {actionId}" };
        if (fromUtc.HasValue)
            filterParts.Add($"OccurredAtUtc ge {fromUtc.Value:yyyy-MM-ddTHH:mm:ssZ}");
        if (toUtc.HasValue)
            filterParts.Add($"OccurredAtUtc le {toUtc.Value:yyyy-MM-ddTHH:mm:ssZ}");
        dateFilter = string.Join(" and ", filterParts);

        var request = new DataGridRequest(1, 10_000, "OccurredAtUtc", false, null);
        var entryResult = await entryService.QueryAsync(request, null, dateFilter);
        var entries = entryResult.Items.ToList();

        var fields = await fieldService.GetByTrackedActionAsync(actionId);

        var stats = BuildActionStats(action, entries, fields);

        var detail = new ActionDetailData
        {
            Stats = stats,
            DailyEntries = BuildDailyEntries(entries)
        };

        // Build time-series for each field
        foreach (var field in fields)
        {
            var entriesWithValues = entries
                .Select(e => (Entry: e, Value: e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == field.Id)?.Value))
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToList();

            switch (field.FieldType)
            {
                case FieldType.Integer:
                case FieldType.Decimal:
                    var numPoints = entriesWithValues
                        .Select(x => decimal.TryParse(x.Value, out var n) ? new NumericDataPoint(x.Entry.OccurredAtUtc, n) : null)
                        .Where(p => p is not null)
                        .Select(p => p!)
                        .ToList();
                    if (numPoints.Count > 0)
                    {
                        detail.NumericSeries.Add(new NumericFieldSeries
                        {
                            Name = field.Name,
                            Unit = field.Unit,
                            Points = numPoints
                        });
                    }
                    break;

                case FieldType.Boolean:
                    var boolByDay = entriesWithValues
                        .GroupBy(x => x.Entry.OccurredAtUtc.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new BooleanDataPoint(
                            g.Key,
                            g.Count(x => x.Value!.Equals("true", StringComparison.OrdinalIgnoreCase)),
                            g.Count(x => !x.Value!.Equals("true", StringComparison.OrdinalIgnoreCase))))
                        .ToList();
                    if (boolByDay.Count > 0)
                    {
                        detail.BooleanSeries.Add(new BooleanFieldSeries
                        {
                            Name = field.Name,
                            Points = boolByDay
                        });
                    }
                    break;

                case FieldType.Dropdown:
                    var dist = entriesWithValues
                        .GroupBy(x => x.Value!)
                        .OrderByDescending(g => g.Count())
                        .Select(g => new DropdownDataPoint(g.Key, g.Count()))
                        .ToList();
                    if (dist.Count > 0)
                    {
                        detail.DropdownSeries.Add(new DropdownFieldSeries
                        {
                            Name = field.Name,
                            Distribution = dist
                        });
                    }
                    break;
            }
        }

        return detail;
    }

    private static List<DailyCount> BuildDailyEntries(IReadOnlyList<ActionEntryResponse> entries)
    {
        if (entries.Count == 0) return [];
        var byDay = entries
            .GroupBy(e => e.OccurredAtUtc.Date)
            .ToDictionary(g => g.Key, g => g.Count());
        var min = byDay.Keys.Min();
        var max = byDay.Keys.Max();
        var result = new List<DailyCount>();
        for (var d = min; d <= max; d = d.AddDays(1))
            result.Add(new DailyCount(d, byDay.GetValueOrDefault(d, 0)));
        return result;
    }

    private static ActionStats BuildActionStats(
        TrackedActionResponse action,
        List<ActionEntryResponse> entries,
        List<ActionFieldResponse> fields)
    {
        var today = DateTime.UtcNow.Date;
        var sorted = entries.OrderByDescending(e => e.OccurredAtUtc).ToList();

        // Streak: consecutive days with at least one entry
        var streak = ComputeStreak(sorted, today);

        // Average gap between entries
        double? avgGapDays = null;
        if (sorted.Count >= 2)
        {
            var gaps = new List<double>();
            for (var i = 0; i < sorted.Count - 1; i++)
            {
                gaps.Add((sorted[i].OccurredAtUtc - sorted[i + 1].OccurredAtUtc).TotalDays);
            }
            avgGapDays = gaps.Average();
        }

        // Entries per week (last 3 months / ~13 weeks)
        var weekBuckets = new List<WeekBucket>();
        for (var w = 12; w >= 0; w--)
        {
            var weekStart = today.AddDays(-((int)today.DayOfWeek) - (w * 7));
            var weekEnd = weekStart.AddDays(7);
            var count = entries.Count(e => e.OccurredAtUtc.Date >= weekStart && e.OccurredAtUtc.Date < weekEnd);
            weekBuckets.Add(new WeekBucket(weekStart, count));
        }

        // Field analytics
        var fieldStats = new List<FieldStats>();
        foreach (var field in fields)
        {
            var values = entries
                .SelectMany(e => e.FieldValues)
                .Where(fv => fv.ActionFieldId == field.Id && !string.IsNullOrWhiteSpace(fv.Value))
                .Select(fv => fv.Value!)
                .ToList();

            fieldStats.Add(BuildFieldStats(field, values));
        }

        // Build numeric time-series for inline charts
        var numericSeries = new List<NumericFieldSeries>();
        foreach (var field in fields.Where(f => f.FieldType is FieldType.Integer or FieldType.Decimal))
        {
            var points = entries
                .Select(e => (e.OccurredAtUtc, Val: e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == field.Id)?.Value))
                .Where(x => !string.IsNullOrWhiteSpace(x.Val) && decimal.TryParse(x.Val, out _))
                .Select(x => new NumericDataPoint(x.OccurredAtUtc, decimal.Parse(x.Val!)))
                .ToList();

            if (points.Count > 0)
            {
                numericSeries.Add(new NumericFieldSeries
                {
                    Name = field.Name,
                    Unit = field.Unit,
                    Points = points
                });
            }
        }

        return new ActionStats
        {
            ActionId = action.Id,
            Name = action.Name,
            Description = action.Description,
            Tags = action.Tags,
            EntryCount = entries.Count,
            FieldCount = action.FieldCount,
            LastEntryUtc = sorted.FirstOrDefault()?.OccurredAtUtc,
            Streak = streak,
            AverageGapDays = avgGapDays,
            WeeklyActivity = weekBuckets,
            FieldStats = fieldStats,
            NumericSeries = numericSeries
        };
    }

    private static FieldStats BuildFieldStats(ActionFieldResponse field, List<string> rawValues)
    {
        var stats = new FieldStats
        {
            FieldId = field.Id,
            Name = field.Name,
            Type = field.FieldType,
            TotalValues = rawValues.Count,
            Unit = field.Unit
        };

        switch (field.FieldType)
        {
            case FieldType.Integer:
            case FieldType.Decimal:
                var numbers = rawValues
                    .Select(v => decimal.TryParse(v, out var n) ? n : (decimal?)null)
                    .Where(n => n.HasValue)
                    .Select(n => n!.Value)
                    .ToList();

                if (numbers.Count > 0)
                {
                    stats.NumericMin = numbers.Min();
                    stats.NumericMax = numbers.Max();
                    stats.NumericAvg = numbers.Average();
                    stats.NumericSum = numbers.Sum();
                    stats.NumericCount = numbers.Count;
                    stats.ConfiguredMin = field.MinValue;
                    stats.ConfiguredMax = field.MaxValue;
                }
                break;

            case FieldType.Boolean:
                var trueCount = rawValues.Count(v => v.Equals("True", StringComparison.OrdinalIgnoreCase));
                var falseCount = rawValues.Count - trueCount;
                stats.BoolTrueCount = trueCount;
                stats.BoolFalseCount = falseCount;
                break;

            case FieldType.Dropdown:
                stats.DropdownDistribution = rawValues
                    .GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .ToDictionary(g => g.Key, g => g.Count());
                break;

            case FieldType.Text:
                stats.TextFilledCount = rawValues.Count;
                break;

            case FieldType.Date:
                stats.TextFilledCount = rawValues.Count;
                break;
        }

        return stats;
    }

    private static int ComputeStreak(List<ActionEntryResponse> sortedDesc, DateTime today)
    {
        if (sortedDesc.Count == 0) return 0;

        var uniqueDays = sortedDesc
            .Select(e => e.OccurredAtUtc.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        // Allow streak to include today or yesterday as the most recent day
        if (uniqueDays[0] != today && uniqueDays[0] != today.AddDays(-1))
            return 0;

        var streak = 1;
        for (var i = 1; i < uniqueDays.Count; i++)
        {
            if ((uniqueDays[i - 1] - uniqueDays[i]).Days == 1)
                streak++;
            else
                break;
        }
        return streak;
    }

    private static int ComputeOverallStreak(IReadOnlyList<ActionEntryResponse> allEntries, DateTime today)
    {
        if (allEntries.Count == 0) return 0;

        var uniqueDays = allEntries
            .Select(e => e.OccurredAtUtc.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (uniqueDays[0] != today && uniqueDays[0] != today.AddDays(-1))
            return 0;

        var streak = 1;
        for (var i = 1; i < uniqueDays.Count; i++)
        {
            if ((uniqueDays[i - 1] - uniqueDays[i]).Days == 1)
                streak++;
            else
                break;
        }
        return streak;
    }
}

public sealed class DashboardData
{
    public int TotalActions { get; init; }
    public int TotalEntries { get; init; }
    public int EntriesToday { get; init; }
    public int EntriesThisWeek { get; init; }
    public int EntriesThisMonth { get; init; }
    public int OverallStreak { get; init; }
    public List<ActionStats> Actions { get; init; } = [];
    public List<DailyCount> GlobalDailyEntries { get; init; } = [];
}

public sealed class ActionStats
{
    public Guid ActionId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public List<TrackedActionTagSummary> Tags { get; init; } = [];
    public int EntryCount { get; init; }
    public int FieldCount { get; init; }
    public DateTime? LastEntryUtc { get; init; }
    public int Streak { get; init; }
    public double? AverageGapDays { get; init; }
    public List<WeekBucket> WeeklyActivity { get; init; } = [];
    public List<FieldStats> FieldStats { get; init; } = [];
    public List<NumericFieldSeries> NumericSeries { get; init; } = [];
}

public sealed record WeekBucket(DateTime WeekStart, int Count);

public sealed class FieldStats
{
    public Guid FieldId { get; init; }
    public required string Name { get; init; }
    public FieldType Type { get; init; }
    public int TotalValues { get; init; }
    public string Unit { get; init; } = "UN";

    // Numeric (Integer/Decimal)
    public decimal? NumericMin { get; set; }
    public decimal? NumericMax { get; set; }
    public decimal? NumericAvg { get; set; }
    public decimal? NumericSum { get; set; }
    public int NumericCount { get; set; }
    public decimal? ConfiguredMin { get; set; }
    public decimal? ConfiguredMax { get; set; }

    // Boolean
    public int BoolTrueCount { get; set; }
    public int BoolFalseCount { get; set; }
    public double BoolTruePercent => BoolTrueCount + BoolFalseCount > 0
        ? Math.Round(BoolTrueCount * 100.0 / (BoolTrueCount + BoolFalseCount), 1)
        : 0;

    // Dropdown
    public Dictionary<string, int> DropdownDistribution { get; set; } = [];

    // Text / Date
    public int TextFilledCount { get; set; }
}

// ── Time-series chart data models ──

public sealed class ActionDetailData
{
    public required ActionStats Stats { get; init; }
    public List<DailyCount> DailyEntries { get; init; } = [];
    public List<NumericFieldSeries> NumericSeries { get; init; } = [];
    public List<BooleanFieldSeries> BooleanSeries { get; init; } = [];
    public List<DropdownFieldSeries> DropdownSeries { get; init; } = [];
}

public sealed record DailyCount(DateTime Date, int Count);

public sealed class NumericFieldSeries
{
    public required string Name { get; init; }
    public required string Unit { get; init; }
    public List<NumericDataPoint> Points { get; init; } = [];
}

public sealed record NumericDataPoint(DateTime Date, decimal Value);

public sealed class BooleanFieldSeries
{
    public required string Name { get; init; }
    public List<BooleanDataPoint> Points { get; init; } = [];
}

public sealed record BooleanDataPoint(DateTime Date, int TrueCount, int FalseCount);

public sealed class DropdownFieldSeries
{
    public required string Name { get; init; }
    public List<DropdownDataPoint> Distribution { get; init; } = [];
}

public sealed class DropdownDataPoint(string Label, int Count)
{
    public string Label { get; } = Label;
    public int Count { get; } = Count;
}
