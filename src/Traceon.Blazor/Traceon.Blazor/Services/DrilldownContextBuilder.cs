using Traceon.Contracts.ActionFields;
using Traceon.Contracts.CustomCharts;
using Traceon.Contracts.Enums;
using Traceon.Contracts.FieldAnalyticsRules;

namespace Traceon.Blazor.Services;

/// <summary>
/// Click signal emitted by <c>CustomChartRenderer</c> when a chart datapoint is selected.
/// At most one of <see cref="GroupKey"/> / <see cref="SeriesLabel"/> is meaningful per click,
/// and <see cref="PointDate"/> is set only for time-series clicks.
/// </summary>
public sealed record ChartClickContext(
    Guid ChartId,
    string? GroupKey,
    string? SeriesLabel,
    DateTime? PointDate);

/// <summary>
/// Composite filter + date range + display strings produced from a chart click.
/// <see cref="LockedFieldFilters"/> holds simple field=value equalities that can
/// be surfaced as read-only inputs in the entries grid; <see cref="Filter"/>
/// holds anything that doesn't fit (IsEmpty, complex operators, Notes sentinel).
/// Both apply on top of the existing field/date filters.
/// </summary>
public sealed record DrilldownContext(
    FilterGroupDto? Filter,
    DateTime? FromUtc,
    DateTime? ToUtc,
    List<string> Chips,
    string Title,
    Dictionary<Guid, string>? LockedFieldFilters = null);

public static class DrilldownContextBuilder
{
    /// <summary>
    /// The literal key used by the chart evaluator for entries that have no value on the group-by field.
    /// Mirrors <c>CustomChartEvaluationService.ExplodeByFieldValues</c>.
    /// </summary>
    private const string EmptyGroupKey = "(empty)";

    public static DrilldownContext Build(
        CustomChartResponse chart,
        CustomChartResult result,
        ChartClickContext click,
        IReadOnlyDictionary<Guid, ActionFieldResponse> fieldMap,
        DateTime? defaultFromUtc = null,
        DateTime? defaultToUtc = null)
    {
        var conditions = new List<FilterConditionDto>();
        var locked = new Dictionary<Guid, string>();
        var chips = new List<string>();

        // Equality leaf for clicked group-by value (bar/pie/donut/table) or series label (time-series).
        var equalityValue = !string.IsNullOrEmpty(click.GroupKey) ? click.GroupKey : click.SeriesLabel;
        if (!string.IsNullOrEmpty(equalityValue) && chart.GroupByFieldId.HasValue)
        {
            var fieldId = chart.GroupByFieldId.Value;
            var fieldName = fieldMap.TryGetValue(fieldId, out var f) ? f.Name : "?";

            if (string.Equals(equalityValue, EmptyGroupKey, StringComparison.Ordinal))
            {
                // IsEmpty doesn't have a single-value field-filter representation; keep as composite.
                conditions.Add(new FilterConditionDto(fieldId, FilterOperator.IsEmpty, null, null));
                chips.Add($"{fieldName}: {EmptyGroupKey}");
            }
            else
            {
                locked[fieldId] = equalityValue;
                chips.Add($"{fieldName}: {equalityValue}");
            }
        }

        // Time bucket → inclusive [from, to] range. The bucket overrides any default period.
        DateTime? fromUtc = null;
        DateTime? toUtc = null;
        if (click.PointDate.HasValue)
        {
            var (rangeFrom, rangeTo) = BucketRange(click.PointDate.Value, result.TimeGrouping);
            fromUtc = rangeFrom;
            toUtc = rangeTo;
            chips.Add(FormatDateRange(rangeFrom, rangeTo, result.TimeGrouping));
        }
        else if (defaultFromUtc.HasValue || defaultToUtc.HasValue)
        {
            fromUtc = defaultFromUtc;
            toUtc = defaultToUtc;
            chips.Add(FormatPeriodChip(defaultFromUtc, defaultToUtc));
        }

        // Combine with the chart's own filter conditions.
        FilterGroupDto? combined;
        var hasNew = conditions.Count > 0;
        if (chart.FilterConditions is not null && hasNew)
        {
            combined = new FilterGroupDto(FilterLogic.And, conditions, [chart.FilterConditions]);
        }
        else if (chart.FilterConditions is not null)
        {
            combined = chart.FilterConditions;
        }
        else if (hasNew)
        {
            combined = new FilterGroupDto(FilterLogic.And, conditions, null);
        }
        else
        {
            combined = null;
        }

        var title = chips.Count > 0
            ? $"{result.Title} — {string.Join(" · ", chips)}"
            : result.Title;

        return new DrilldownContext(combined, fromUtc, toUtc, chips, title,
            locked.Count > 0 ? locked : null);
    }

    /// <summary>
    /// Build a drilldown context for a CrossField analytics rule click. CrossField time-series are
    /// always bucketed per-day, so <paramref name="pointDate"/> expands to that day's range.
    /// </summary>
    /// <param name="groupKey">Top-level group value (the rule's GroupByField). Null if not a group click.</param>
    /// <param name="subGroupKey">Sub-group value (the rule's MeasureField, used for CountByValue/AggregateByValue with a dropdown measure). Null otherwise.</param>
    /// <param name="pointDate">Time-series bucket date. Null for non-time-series clicks.</param>
    public static DrilldownContext BuildCrossField(
        FieldAnalyticsRuleResponse rule,
        string title,
        string? groupKey,
        string? subGroupKey,
        DateTime? pointDate,
        IReadOnlyDictionary<Guid, ActionFieldResponse> fieldMap,
        DateTime? defaultFromUtc = null,
        DateTime? defaultToUtc = null)
    {
        var conditions = new List<FilterConditionDto>();
        var locked = new Dictionary<Guid, string>();
        var chips = new List<string>();

        // Rule's own filter (e.g. "Tipo = Despesa"). If FilterMetadataFieldId is set the filter
        // operates against a dropdown's metadata column, which the entries grid can't reproduce
        // exactly — surface as a display-only chip in that case so the user sees the discrepancy.
        if (rule.FilterFieldId.HasValue && !string.IsNullOrEmpty(rule.FilterValue))
        {
            var filterFieldName = rule.FilterFieldName ?? "?";
            if (rule.FilterMetadataFieldId.HasValue)
            {
                chips.Add($"{filterFieldName}: {rule.FilterValue} (metadata)");
            }
            else
            {
                locked[rule.FilterFieldId.Value] = rule.FilterValue;
                chips.Add($"{filterFieldName}: {rule.FilterValue}");
            }
        }

        // Top-level group equality (e.g. Categoria = Transferências).
        if (!string.IsNullOrEmpty(groupKey))
        {
            var fieldName = fieldMap.TryGetValue(rule.GroupByFieldId, out var f) ? f.Name : rule.GroupByFieldName;
            if (string.Equals(groupKey, EmptyGroupKey, StringComparison.Ordinal))
            {
                conditions.Add(new FilterConditionDto(rule.GroupByFieldId, FilterOperator.IsEmpty, null, null));
                chips.Add($"{fieldName}: {EmptyGroupKey}");
            }
            else
            {
                locked[rule.GroupByFieldId] = groupKey;
                chips.Add($"{fieldName}: {groupKey}");
            }
        }

        // Sub-group equality on the measure field (CountByValue/AggregateByValue with dropdown measure).
        if (!string.IsNullOrEmpty(subGroupKey))
        {
            var fieldName = fieldMap.TryGetValue(rule.MeasureFieldId, out var f) ? f.Name : rule.MeasureFieldName;
            locked[rule.MeasureFieldId] = subGroupKey;
            chips.Add($"{fieldName}: {subGroupKey}");
        }

        // CrossField time-series buckets are always one day. A specific bucket date overrides
        // any dashboard-level period; otherwise carry the dashboard period through.
        DateTime? fromUtc = null;
        DateTime? toUtc = null;
        if (pointDate.HasValue)
        {
            var (rangeFrom, rangeTo) = BucketRange(pointDate.Value, TimeGrouping.Day);
            fromUtc = rangeFrom;
            toUtc = rangeTo;
            chips.Add(rangeFrom.ToLocalTime().ToString("yyyy-MM-dd"));
        }
        else if (defaultFromUtc.HasValue || defaultToUtc.HasValue)
        {
            fromUtc = defaultFromUtc;
            toUtc = defaultToUtc;
            chips.Add(FormatPeriodChip(defaultFromUtc, defaultToUtc));
        }

        FilterGroupDto? combined = conditions.Count > 0
            ? new FilterGroupDto(FilterLogic.And, conditions, null)
            : null;

        var resolvedTitle = chips.Count > 0
            ? $"{title} — {string.Join(" · ", chips)}"
            : title;

        return new DrilldownContext(combined, fromUtc, toUtc, chips, resolvedTitle,
            locked.Count > 0 ? locked : null);
    }

    /// <summary>
    /// Build a drilldown context for a DropdownValueTrend chart click. The chart shows a numeric
    /// field's evolution per dropdown value over time (per-day buckets), so a click yields:
    /// <paramref name="dropdownFieldId"/> = <paramref name="valueLabel"/> + the clicked day.
    /// </summary>
    /// <summary>
    /// Build a drilldown context for a dropdown distribution chart click. Locks the dropdown
    /// field to the clicked value and carries the dashboard period through, since the donut
    /// aggregates the whole filtered window.
    /// </summary>
    public static DrilldownContext BuildDropdownDistribution(
        string fieldName,
        Guid fieldId,
        string valueLabel,
        DateTime? defaultFromUtc,
        DateTime? defaultToUtc)
    {
        var locked = new Dictionary<Guid, string> { [fieldId] = valueLabel };
        var chips = new List<string> { $"{fieldName}: {valueLabel}" };
        if (defaultFromUtc.HasValue || defaultToUtc.HasValue)
            chips.Add(FormatPeriodChip(defaultFromUtc, defaultToUtc));
        var title = $"{fieldName} — {valueLabel}";
        return new DrilldownContext(null, defaultFromUtc, defaultToUtc, chips, title, locked);
    }

    /// <summary>
    /// Build a drilldown context for a boolean stacked-bar click. Locks the field to the
    /// clicked side (true/false bar) and pins the local-day range.
    /// </summary>
    public static DrilldownContext BuildBooleanTrend(
        string fieldName,
        Guid fieldId,
        bool valueIsTrue,
        DateTime pointDate)
    {
        var (rangeFrom, rangeTo) = BucketRange(pointDate, TimeGrouping.Day);
        var localFrom = rangeFrom.ToLocalTime();
        // The field-filter UI binds equality on the raw stored value, which is "True"/"False"
        // (matches BuildFieldStats / BoolTrueCount comparison). Match that casing.
        var rawValue = valueIsTrue ? "True" : "False";
        var locked = new Dictionary<Guid, string> { [fieldId] = rawValue };
        var chips = new List<string>
        {
            $"{fieldName}: {rawValue}",
            localFrom.ToString("yyyy-MM-dd")
        };
        var title = $"{fieldName} = {rawValue} — {localFrom:yyyy-MM-dd}";
        return new DrilldownContext(null, rangeFrom, rangeTo, chips, title, locked);
    }

    /// <summary>
    /// Build a drilldown context for a per-field numeric/boolean trend chart. Filters to entries
    /// that have a value on <paramref name="fieldId"/> (so empties don't pollute the result) on
    /// the day of <paramref name="pointDate"/>.
    /// </summary>
    public static DrilldownContext BuildFieldTrend(
        string fieldName,
        Guid fieldId,
        string chartTitle,
        DateTime pointDate)
    {
        var (rangeFrom, rangeTo) = BucketRange(pointDate, TimeGrouping.Day);
        var localFrom = rangeFrom.ToLocalTime();
        var conditions = new List<FilterConditionDto>
        {
            new(fieldId, FilterOperator.IsNotEmpty, null, null)
        };
        var chips = new List<string>
        {
            $"{fieldName}: ≠ ∅",
            localFrom.ToString("yyyy-MM-dd")
        };
        var title = $"{chartTitle} — {localFrom:yyyy-MM-dd}";
        var combined = new FilterGroupDto(FilterLogic.And, conditions, null);
        return new DrilldownContext(combined, rangeFrom, rangeTo, chips, title, null);
    }

    /// <summary>
    /// Build a drilldown context for the per-action Entry Frequency chart. No field filter
    /// is applied — the entries grid is already scoped to the action; we only pin the day.
    /// </summary>
    public static DrilldownContext BuildEntryFrequency(string title, DateTime pointDate)
    {
        var (rangeFrom, rangeTo) = BucketRange(pointDate, TimeGrouping.Day);
        var localFrom = rangeFrom.ToLocalTime();
        var chips = new List<string> { localFrom.ToString("yyyy-MM-dd") };
        var resolvedTitle = $"{title} — {localFrom:yyyy-MM-dd}";
        return new DrilldownContext(null, rangeFrom, rangeTo, chips, resolvedTitle, null);
    }

    public static DrilldownContext BuildDropdownValueTrend(
        string fieldName,
        string valueFieldName,
        Guid dropdownFieldId,
        string valueLabel,
        DateTime pointDate)
    {
        var locked = new Dictionary<Guid, string> { [dropdownFieldId] = valueLabel };
        var (rangeFrom, rangeTo) = BucketRange(pointDate, TimeGrouping.Day);
        var localFrom = rangeFrom.ToLocalTime();
        var chips = new List<string>
        {
            $"{fieldName}: {valueLabel}",
            localFrom.ToString("yyyy-MM-dd")
        };
        var title = $"{fieldName} — {valueFieldName} · {valueLabel} · {localFrom:yyyy-MM-dd}";
        return new DrilldownContext(null, rangeFrom, rangeTo, chips, title, locked);
    }

    // The bucketStart is the chart's bucket key, computed from a local-time entry timestamp,
    // so it represents local-day midnight. We expand it to a local-day range and only convert
    // to UTC at the end, since the consumers (FromUtc/ToUtc) feed an OData filter on a UTC column.
    private static (DateTime From, DateTime To) BucketRange(DateTime bucketStart, TimeGrouping grouping)
    {
        var localStart = grouping switch
        {
            TimeGrouping.Day => DateTime.SpecifyKind(bucketStart.Date, DateTimeKind.Local),
            TimeGrouping.Week => DateTime.SpecifyKind(bucketStart.Date, DateTimeKind.Local),
            TimeGrouping.Month => DateTime.SpecifyKind(new DateTime(bucketStart.Year, bucketStart.Month, 1), DateTimeKind.Local),
            TimeGrouping.Year => DateTime.SpecifyKind(new DateTime(bucketStart.Year, 1, 1), DateTimeKind.Local),
            _ => DateTime.SpecifyKind(bucketStart.Date, DateTimeKind.Local)
        };
        var localEnd = grouping switch
        {
            TimeGrouping.Day => localStart.AddDays(1).AddTicks(-1),
            TimeGrouping.Week => localStart.AddDays(7).AddTicks(-1),
            TimeGrouping.Month => localStart.AddMonths(1).AddTicks(-1),
            TimeGrouping.Year => localStart.AddYears(1).AddTicks(-1),
            _ => localStart.AddDays(1).AddTicks(-1)
        };
        return (localStart.ToUniversalTime(), localEnd.ToUniversalTime());
    }

    private static string FormatDateRange(DateTime from, DateTime to, TimeGrouping grouping)
    {
        var localFrom = from.ToLocalTime();
        var localTo = to.ToLocalTime();
        return grouping switch
        {
            TimeGrouping.Day => localFrom.ToString("yyyy-MM-dd"),
            TimeGrouping.Week => $"{localFrom:yyyy-MM-dd} – {localTo:yyyy-MM-dd}",
            TimeGrouping.Month => localFrom.ToString("yyyy-MM"),
            TimeGrouping.Year => localFrom.ToString("yyyy"),
            _ => $"{localFrom:yyyy-MM-dd} – {localTo:yyyy-MM-dd}"
        };
    }

    private static string FormatPeriodChip(DateTime? from, DateTime? to) => (from?.ToLocalTime(), to?.ToLocalTime()) switch
    {
        ({ } f, { } t) => $"{f:yyyy-MM-dd} – {t:yyyy-MM-dd}",
        ({ } f, null) => $"≥ {f:yyyy-MM-dd}",
        (null, { } t) => $"≤ {t:yyyy-MM-dd}",
        _ => string.Empty
    };
}
