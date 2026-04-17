using System.Globalization;
using Traceon.Contracts.ActionEntries;
using Traceon.Contracts.ActionFields;
using Traceon.Contracts.CustomCharts;
using Traceon.Contracts.Enums;

namespace Traceon.Blazor.Services;

public static class CustomChartEvaluationService
{
    public static CustomChartResult? Evaluate(
        CustomChartResponse chart,
        List<ActionEntryResponse> entries,
        List<ActionFieldResponse> fields)
    {
        var fieldMap = fields.ToDictionary(f => f.Id);

        // 1. Apply composite filter
        var filtered = ApplyFilters(entries, chart.FilterConditions, fieldMap);
        if (filtered.Count == 0) return null;

        // 2. Group entries
        var grouped = GroupEntries(filtered, chart, fieldMap);

        // 3. Aggregate each group
        var resultGroups = AggregateGroups(grouped, chart, fieldMap);

        // 4. Sort
        resultGroups = chart.SortDescending
            ? resultGroups.OrderByDescending(g => g.Value).ToList()
            : resultGroups.OrderBy(g => g.Key).ToList();

        // 5. Limit groups
        if (chart.MaxGroups.HasValue && chart.MaxGroups.Value > 0)
            resultGroups = resultGroups.Take(chart.MaxGroups.Value).ToList();

        // 6. Build time series if needed
        List<CrossFieldTimeSeries> timeSeriesData = [];
        if (chart.ChartType == CustomChartType.TimeSeries && chart.TimeGrouping != TimeGrouping.None)
        {
            timeSeriesData = BuildTimeSeries(filtered, chart, fieldMap);
        }

        var measureField = fieldMap.GetValueOrDefault(chart.MeasureFieldId);
        var groupByField = chart.GroupByFieldId.HasValue ? fieldMap.GetValueOrDefault(chart.GroupByFieldId.Value) : null;

        return new CustomChartResult
        {
            ChartId = chart.Id,
            Title = chart.Title,
            ChartType = chart.ChartType,
            Aggregation = chart.Aggregation,
            Unit = measureField?.Unit,
            MeasureFieldName = chart.MeasureFieldName,
            GroupByFieldName = groupByField?.Name,
            TimeGrouping = chart.TimeGrouping,
            ColorPalette = chart.ColorPalette,
            SortDescending = chart.SortDescending,
            ShowTotalizer = chart.ShowTotalizer,
            Groups = resultGroups,
            TimeSeriesData = timeSeriesData
        };
    }

    // ── Filtering ──

    private static List<ActionEntryResponse> ApplyFilters(
        List<ActionEntryResponse> entries,
        FilterGroupDto? filterGroup,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        if (filterGroup is null) return entries;
        return entries.Where(e => EvaluateGroup(e, filterGroup, fieldMap)).ToList();
    }

    private static bool EvaluateGroup(
        ActionEntryResponse entry,
        FilterGroupDto group,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        var results = new List<bool>();

        if (group.Conditions is not null)
        {
            foreach (var condition in group.Conditions)
                results.Add(EvaluateCondition(entry, condition, fieldMap));
        }

        if (group.Groups is not null)
        {
            foreach (var subGroup in group.Groups)
                results.Add(EvaluateGroup(entry, subGroup, fieldMap));
        }

        if (results.Count == 0) return true;

        return group.Logic == FilterLogic.And
            ? results.All(r => r)
            : results.Any(r => r);
    }

    private static bool EvaluateCondition(
        ActionEntryResponse entry,
        FilterConditionDto condition,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        var rawValue = entry.FieldValues
            .FirstOrDefault(fv => fv.ActionFieldId == condition.FieldId)?.Value;

        var fieldType = fieldMap.TryGetValue(condition.FieldId, out var field)
            ? field.FieldType
            : FieldType.Text;

        return condition.Operator switch
        {
            FilterOperator.Equals => StringEquals(rawValue, condition.Value),
            FilterOperator.NotEquals => !StringEquals(rawValue, condition.Value),
            FilterOperator.Contains => rawValue?.Contains(condition.Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            FilterOperator.NotContains => rawValue?.Contains(condition.Value ?? "", StringComparison.OrdinalIgnoreCase) != true,
            FilterOperator.StartsWith => rawValue?.StartsWith(condition.Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            FilterOperator.EndsWith => rawValue?.EndsWith(condition.Value ?? "", StringComparison.OrdinalIgnoreCase) == true,
            FilterOperator.IsEmpty => string.IsNullOrWhiteSpace(rawValue),
            FilterOperator.IsNotEmpty => !string.IsNullOrWhiteSpace(rawValue),
            FilterOperator.GreaterThan => CompareValues(rawValue, condition.Value, fieldType) > 0,
            FilterOperator.GreaterThanOrEqual => CompareValues(rawValue, condition.Value, fieldType) >= 0,
            FilterOperator.LessThan => CompareValues(rawValue, condition.Value, fieldType) < 0,
            FilterOperator.LessThanOrEqual => CompareValues(rawValue, condition.Value, fieldType) <= 0,
            FilterOperator.Between => IsBetween(rawValue, condition.Value, condition.ValueTo, fieldType),
            FilterOperator.In => IsIn(rawValue, condition.Value),
            FilterOperator.NotIn => !IsIn(rawValue, condition.Value),
            _ => true
        };
    }

    private static bool StringEquals(string? a, string? b)
        => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static int CompareValues(string? rawValue, string? compareValue, FieldType fieldType)
    {
        if (rawValue is null) return -1;
        if (compareValue is null) return 1;

        if (fieldType is FieldType.Integer or FieldType.Decimal)
        {
            if (decimal.TryParse(rawValue, CultureInfo.InvariantCulture, out var numA)
                && decimal.TryParse(compareValue, CultureInfo.InvariantCulture, out var numB))
                return numA.CompareTo(numB);
        }

        if (fieldType == FieldType.Date)
        {
            if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateA)
                && DateTime.TryParse(compareValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateB))
                return dateA.CompareTo(dateB);
        }

        return string.Compare(rawValue, compareValue, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBetween(string? rawValue, string? from, string? to, FieldType fieldType)
    {
        if (rawValue is null || from is null || to is null) return false;
        return CompareValues(rawValue, from, fieldType) >= 0
            && CompareValues(rawValue, to, fieldType) <= 0;
    }

    private static bool IsIn(string? rawValue, string? values)
    {
        if (rawValue is null || values is null) return false;
        var set = values.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return set.Any(v => string.Equals(v, rawValue.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    // ── Grouping ──

    private static Dictionary<string, List<ActionEntryResponse>> GroupEntries(
        List<ActionEntryResponse> entries,
        CustomChartResponse chart,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        if (chart.GroupByFieldId.HasValue)
        {
            // Group by field value
            return entries
                .GroupBy(e =>
                {
                    var val = e.FieldValues
                        .FirstOrDefault(fv => fv.ActionFieldId == chart.GroupByFieldId.Value)?.Value;
                    return string.IsNullOrWhiteSpace(val) ? "(empty)" : val.Trim();
                })
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        if (chart.TimeGrouping != TimeGrouping.None)
        {
            // Group by time bucket
            return entries
                .GroupBy(e => GetTimeBucketKey(e.OccurredAtUtc, chart.TimeGrouping))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // No grouping — everything in one bucket
        return new Dictionary<string, List<ActionEntryResponse>> { ["All"] = entries };
    }

    private static string GetTimeBucketKey(DateTime date, TimeGrouping grouping) => grouping switch
    {
        TimeGrouping.Day => date.ToString("yyyy-MM-dd"),
        TimeGrouping.Week => $"{ISOWeek(date):yyyy}-W{ISOWeek(date).week:D2}",
        TimeGrouping.Month => date.ToString("yyyy-MM"),
        TimeGrouping.Year => date.ToString("yyyy"),
        _ => date.ToString("yyyy-MM-dd")
    };

    private static (int year, int week) ISOWeek(DateTime date)
    {
        var week = ISOWeekNumber(date);
        return (date.Year, week);
    }

    private static int ISOWeekNumber(DateTime date)
    {
        var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            date = date.AddDays(3);
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
            date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private static DateTime GetTimeBucketDate(DateTime date, TimeGrouping grouping) => grouping switch
    {
        TimeGrouping.Day => date.Date,
        TimeGrouping.Week => date.Date.AddDays(-(int)(date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1)),
        TimeGrouping.Month => new DateTime(date.Year, date.Month, 1),
        TimeGrouping.Year => new DateTime(date.Year, 1, 1),
        _ => date.Date
    };

    // ── Aggregation ──

    private static List<CrossFieldGroup> AggregateGroups(
        Dictionary<string, List<ActionEntryResponse>> grouped,
        CustomChartResponse chart,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        var results = new List<CrossFieldGroup>();

        foreach (var (key, groupEntries) in grouped)
        {
            var value = AggregateValue(groupEntries, chart.MeasureFieldId, chart.Aggregation);
            results.Add(new CrossFieldGroup(key, value));
        }

        return results;
    }

    private static decimal AggregateValue(
        List<ActionEntryResponse> entries,
        Guid measureFieldId,
        AnalyticsAggregation aggregation)
    {
        if (aggregation == AnalyticsAggregation.Count)
            return entries.Count;

        var values = entries
            .Select(e => e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == measureFieldId)?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => decimal.TryParse(v, CultureInfo.InvariantCulture, out var d) ? (decimal?)d : null)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .ToList();

        if (values.Count == 0) return 0;

        return aggregation switch
        {
            AnalyticsAggregation.Sum => values.Sum(),
            AnalyticsAggregation.Avg => values.Average(),
            AnalyticsAggregation.Min => values.Min(),
            AnalyticsAggregation.Max => values.Max(),
            _ => values.Sum()
        };
    }

    // ── Time Series ──

    private static List<CrossFieldTimeSeries> BuildTimeSeries(
        List<ActionEntryResponse> entries,
        CustomChartResponse chart,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        if (chart.GroupByFieldId.HasValue)
        {
            // Multiple series: one per group-by value
            var byGroup = entries
                .GroupBy(e =>
                {
                    var val = e.FieldValues
                        .FirstOrDefault(fv => fv.ActionFieldId == chart.GroupByFieldId.Value)?.Value;
                    return string.IsNullOrWhiteSpace(val) ? "(empty)" : val.Trim();
                })
                .Take(15); // limit series count

            return byGroup.Select(g => new CrossFieldTimeSeries
            {
                SeriesLabel = g.Key,
                Points = BuildTimeSeriesPoints(g.ToList(), chart)
            }).ToList();
        }

        // Single series
        return
        [
            new CrossFieldTimeSeries
            {
                SeriesLabel = chart.MeasureFieldName,
                Points = BuildTimeSeriesPoints(entries, chart)
            }
        ];
    }

    private static List<NumericDataPoint> BuildTimeSeriesPoints(
        List<ActionEntryResponse> entries,
        CustomChartResponse chart)
    {
        return entries
            .GroupBy(e => GetTimeBucketDate(e.OccurredAtUtc, chart.TimeGrouping))
            .OrderBy(g => g.Key)
            .Select(g => new NumericDataPoint(
                g.Key,
                AggregateValue(g.ToList(), chart.MeasureFieldId, chart.Aggregation)))
            .ToList();
    }
}

// ── Result Model ──

public sealed class CustomChartResult
{
    public required Guid ChartId { get; init; }
    public required string Title { get; init; }
    public required CustomChartType ChartType { get; init; }
    public required AnalyticsAggregation Aggregation { get; init; }
    public string? Unit { get; init; }
    public string? MeasureFieldName { get; init; }
    public string? GroupByFieldName { get; init; }
    public TimeGrouping TimeGrouping { get; init; }
    public string? ColorPalette { get; init; }
    public bool SortDescending { get; init; }
    public bool ShowTotalizer { get; init; }
    public List<CrossFieldGroup> Groups { get; init; } = [];
    public List<CrossFieldTimeSeries> TimeSeriesData { get; init; } = [];
    public decimal Total => Groups.Sum(g => g.Value);
}
