using System.Globalization;
using Traceon.Blazor.Helpers;
using Traceon.Blazor.Components;
using Traceon.Contracts.ActionEntries;
using Traceon.Contracts.ActionFields;
using Traceon.Contracts.DropdownValues;
using Traceon.Contracts.Enums;
using Traceon.Contracts.FieldAnalyticsRules;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Blazor.Services;

public sealed class DashboardService(
    TrackedActionService actionService,
    ActionEntryService entryService,
    ActionFieldService fieldService,
    FieldAnalyticsRuleService analyticsRuleService,
    DropdownValueService dropdownValueService)
{
    /// <summary>
    /// Lookup keyed by the ActionField-parent FieldDefinitionId, mapping dropdown value text
    /// to a per-metadata-field dictionary. Used to resolve metadata-based group-by and filter
    /// in cross-field analytics.
    /// </summary>
    private sealed record MetadataLookup(IReadOnlyDictionary<Guid, Dictionary<string, Dictionary<Guid, string?>>> ByFieldDefinitionId);
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

            var stats = BuildActionStats(action, entries, fields ?? []);

            // Compute running balances from SignedSum analytics rules
            if (fields is { Count: > 0 } && entries.Count > 0)
            {
                try
                {
                    var rules = await analyticsRuleService.GetByTrackedActionAsync(action.Id);
                    var signedSumRules = rules.Where(r => r.Aggregation == AnalyticsAggregation.SignedSum).ToList();
                    if (signedSumRules.Count > 0)
                    {
                        var fieldMap = fields.ToDictionary(f => f.Id);
                        stats.Balances = BuildBalanceSummaries(signedSumRules, entries, fieldMap);
                    }
                }
                catch { /* analytics are optional */ }

                // Also add balance summaries for goal-based numeric fields without SignedSum rules
                var coveredIds = stats.Balances.Select(b => b.MeasureFieldId).Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();
                foreach (var field in fields.Where(f =>
                    f.FieldType is FieldType.Integer or FieldType.Decimal &&
                    f.TargetValueMode == TargetValueMode.Goal &&
                    f.TargetValue.HasValue &&
                    !coveredIds.Contains(f.Id)))
                {
                    var total = entries
                        .Select(e => e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == field.Id)?.Value)
                        .Where(v => decimal.TryParse(v, CultureInfo.InvariantCulture, out _))
                        .Sum(v => decimal.Parse(v!, CultureInfo.InvariantCulture));

                    var unit = field.Unit is not null && field.Unit != "UN" ? field.Unit : "";
                    stats.Balances.Add(new BalanceSummary
                    {
                        Label = field.Name,
                        Unit = unit,
                        CurrentBalance = total,
                        GoalValue = field.TargetValue,
                        MeasureFieldId = field.Id
                    });
                }
            }

            actionStats.Add(stats);
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
                    var rawPoints = entriesWithValues
                        .Select(x => decimal.TryParse(x.Value, CultureInfo.InvariantCulture, out var n) ? new NumericDataPoint(x.Entry.OccurredAtUtc, n) : null)
                        .Where(p => p is not null)
                        .Select(p => p!)
                        .ToList();

                    var numPoints = AggregateTrendPoints(rawPoints, field.TrendAggregation);

                    if (numPoints.Count > 0)
                    {
                        detail.NumericSeries.Add(new NumericFieldSeries
                        {
                            Name = field.Name,
                            Unit = field.Unit,
                            IsInteger = field.FieldType == FieldType.Integer,
                            Points = numPoints,
                            ChartType = field.TrendChartType,
                            TargetValue = field.TargetValue,
                            TargetValueMode = field.TargetValueMode
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
                case FieldType.CompositeDropdown:
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

                    // Dropdown value trend chart (one time-series per dropdown value)
                    if (field.DropdownTrendValueFieldId.HasValue)
                    {
                        var valueField = fields.FirstOrDefault(f => f.Id == field.DropdownTrendValueFieldId.Value);
                        if (valueField is not null)
                        {
                            var trendSeries = BuildDropdownValueTrend(field, valueField, entries);
                            if (trendSeries is not null)
                                detail.DropdownValueTrends.Add(trendSeries);
                        }
                    }
                    break;
            }
        }

        // Build cross-field analytics from configured rules
        try
        {
            var rules = await analyticsRuleService.GetByTrackedActionAsync(actionId);
            if (rules.Count > 0)
            {
                var fieldMap = fields.ToDictionary(f => f.Id);
                var metadataLookup = await BuildMetadataLookupAsync(rules, fieldMap);
                detail.CrossFieldResults = BuildCrossFieldResults(rules, entries, fieldMap, metadataLookup);
                detail.RunningBalances = BuildRunningBalances(rules, entries, fieldMap);
            }
        }
        catch
        {
            // Analytics rules are optional; don't fail the whole detail load
        }

        // Build goal-based running balances for numeric fields with TargetValueMode == Goal
        // that aren't already covered by a SignedSum analytics rule
        var coveredFieldIds = detail.RunningBalances.Count > 0
            ? detail.RunningBalances.Select(b => b.MeasureFieldId).Where(id => id.HasValue).Select(id => id!.Value).ToHashSet()
            : new HashSet<Guid>();

        foreach (var field in fields.Where(f =>
            f.FieldType is FieldType.Integer or FieldType.Decimal &&
            f.TargetValueMode == TargetValueMode.Goal &&
            f.TargetValue.HasValue &&
            !coveredFieldIds.Contains(f.Id)))
        {
            var chronoEntries = entries
                .OrderBy(e => e.OccurredAtUtc)
                .Select(e => (Date: e.OccurredAtUtc, Val: e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == field.Id)?.Value))
                .Where(x => decimal.TryParse(x.Val, CultureInfo.InvariantCulture, out _))
                .ToList();

            if (chronoEntries.Count == 0) continue;

            var points = new List<NumericDataPoint>();
            decimal runningTotal = 0;
            foreach (var entry in chronoEntries)
            {
                runningTotal += decimal.Parse(entry.Val!, CultureInfo.InvariantCulture);
                points.Add(new NumericDataPoint(entry.Date, runningTotal));
            }

            var unit = field.Unit is not null && field.Unit != "UN" ? field.Unit : "";
            detail.RunningBalances.Add(new RunningBalanceSeries
            {
                Label = field.Name,
                Unit = unit,
                CurrentBalance = runningTotal,
                GoalValue = field.TargetValue,
                MeasureFieldId = field.Id,
                Points = points
            });
        }

        return detail;
    }

    private async Task<MetadataLookup> BuildMetadataLookupAsync(
        List<FieldAnalyticsRuleResponse> rules,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        var fieldDefIds = new HashSet<Guid>();
        foreach (var rule in rules)
        {
            if (rule.GroupByMetadataFieldId.HasValue && fieldMap.TryGetValue(rule.GroupByFieldId, out var gbf))
                fieldDefIds.Add(gbf.FieldDefinitionId);
            if (rule.FilterMetadataFieldId.HasValue && rule.FilterFieldId.HasValue &&
                fieldMap.TryGetValue(rule.FilterFieldId.Value, out var ff))
                fieldDefIds.Add(ff.FieldDefinitionId);
        }

        var lookup = new Dictionary<Guid, Dictionary<string, Dictionary<Guid, string?>>>();
        foreach (var fieldDefId in fieldDefIds)
        {
            var ddValues = await dropdownValueService.GetByFieldDefinitionIdAsync(fieldDefId);
            var byValueText = new Dictionary<string, Dictionary<Guid, string?>>(StringComparer.OrdinalIgnoreCase);
            foreach (var dv in ddValues)
            {
                var metaMap = dv.Metadata.ToDictionary(m => m.MetadataFieldId, m => m.Value);
                byValueText[dv.Value] = metaMap;
            }
            lookup[fieldDefId] = byValueText;
        }

        return new MetadataLookup(lookup);
    }

    private static string? ResolveMetadataValue(
        MetadataLookup lookup,
        Guid fieldDefinitionId,
        string? dropdownValueText,
        Guid metadataFieldId)
    {
        if (string.IsNullOrWhiteSpace(dropdownValueText)) return null;
        if (!lookup.ByFieldDefinitionId.TryGetValue(fieldDefinitionId, out var byText)) return null;
        if (!byText.TryGetValue(dropdownValueText, out var metaMap)) return null;
        return metaMap.GetValueOrDefault(metadataFieldId);
    }

    private static List<CrossFieldResult> BuildCrossFieldResults(
        List<FieldAnalyticsRuleResponse> rules,
        List<ActionEntryResponse> entries,
        Dictionary<Guid, ActionFieldResponse> fieldMap,
        MetadataLookup metadataLookup)
    {
        var results = new List<CrossFieldResult>();

        foreach (var rule in rules)
        {
            if (!fieldMap.TryGetValue(rule.MeasureFieldId, out var measureField) ||
                !fieldMap.TryGetValue(rule.GroupByFieldId, out var groupByField))
                continue;

            // Optionally filter entries by the filter field
            var filteredEntries = entries;
            if (rule.FilterFieldId.HasValue && rule.FilterValue is not null)
            {
                var filterField = fieldMap.GetValueOrDefault(rule.FilterFieldId.Value);
                filteredEntries = entries
                    .Where(e =>
                    {
                        var raw = e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == rule.FilterFieldId.Value)?.Value;
                        var resolved = rule.FilterMetadataFieldId.HasValue && filterField is not null
                            ? ResolveMetadataValue(metadataLookup, filterField.FieldDefinitionId, raw, rule.FilterMetadataFieldId.Value)
                            : raw;
                        return string.Equals(resolved, rule.FilterValue, StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();
            }

            // Extract (date, groupKey, measureValue) tuples from each entry
            var pairs = filteredEntries
                .Select(e =>
                {
                    var rawGroupVal = e.FieldValues
                        .FirstOrDefault(fv => fv.ActionFieldId == rule.GroupByFieldId)?.Value;
                    var groupVal = rule.GroupByMetadataFieldId.HasValue
                        ? ResolveMetadataValue(metadataLookup, groupByField.FieldDefinitionId, rawGroupVal, rule.GroupByMetadataFieldId.Value)
                        : rawGroupVal;
                    var measureVal = e.FieldValues
                        .FirstOrDefault(fv => fv.ActionFieldId == rule.MeasureFieldId)?.Value;
                    var signVal = rule.SignFieldId.HasValue
                        ? e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == rule.SignFieldId.Value)?.Value
                        : null;
                    return (Date: e.OccurredAtUtc, GroupKey: groupVal, MeasureValue: measureVal, SignValue: signVal);
                })
                .Where(p => !string.IsNullOrWhiteSpace(p.GroupKey))
                .ToList();

            if (pairs.Count == 0)
                continue;

            // Parse negative values for SignedSum
            HashSet<string>? negativeValueSet = null;
            if (rule.Aggregation == AnalyticsAggregation.SignedSum && rule.NegativeValues is not null)
            {
                negativeValueSet = DropdownValuesHelper.Split(rule.NegativeValues)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            var isNumericMeasure = measureField.FieldType is FieldType.Integer or FieldType.Decimal;
            var isDropdownMeasure = measureField.FieldType is FieldType.Dropdown or FieldType.CompositeDropdown;
            var groups = new List<CrossFieldGroup>();

            if (rule.Aggregation == AnalyticsAggregation.CountByValue && isDropdownMeasure)
            {
                // Parse requested metrics (stored in NegativeValues for CountByValue)
                var requestedMetrics = DropdownValuesHelper.Split(rule.NegativeValues)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var hasValueField = rule.SignFieldId.HasValue;

                // Hierarchical: group by GroupByField, then count each MeasureField value within
                foreach (var grp in pairs.GroupBy(p => p.GroupKey!).OrderBy(g => g.Key))
                {
                    var subGroups = grp
                        .Where(p => !string.IsNullOrWhiteSpace(p.MeasureValue))
                        .GroupBy(p => p.MeasureValue!, StringComparer.OrdinalIgnoreCase)
                        .OrderByDescending(sg => sg.Count())
                        .Select(sg =>
                        {
                            Dictionary<string, decimal>? subMetrics = null;
                            if (hasValueField && requestedMetrics.Count > 0)
                            {
                                var nums = sg
                                    .Where(p => decimal.TryParse(p.SignValue, CultureInfo.InvariantCulture, out _))
                                    .Select(p => decimal.Parse(p.SignValue!, CultureInfo.InvariantCulture))
                                    .ToList();
                                if (nums.Count > 0)
                                    subMetrics = ComputeMetrics(nums, requestedMetrics);
                            }
                            return new CrossFieldGroup(sg.Key, sg.Count()) { Metrics = subMetrics };
                        })
                        .ToList();

                    Dictionary<string, decimal>? grpMetrics = null;
                    if (hasValueField && requestedMetrics.Count > 0)
                    {
                        var allNums = grp
                            .Where(p => decimal.TryParse(p.SignValue, CultureInfo.InvariantCulture, out _))
                            .Select(p => decimal.Parse(p.SignValue!, CultureInfo.InvariantCulture))
                            .ToList();
                        if (allNums.Count > 0)
                            grpMetrics = ComputeMetrics(allNums, requestedMetrics);
                    }

                    groups.Add(new CrossFieldGroup(grp.Key!, grp.Count()) { SubGroups = subGroups, Metrics = grpMetrics });
                }
            }
            else
            {
            foreach (var grp in pairs.GroupBy(p => p.GroupKey!).OrderBy(g => g.Key))
            {
                decimal aggregatedValue;

                if (rule.Aggregation == AnalyticsAggregation.Count)
                {
                    aggregatedValue = grp.Count();
                }
                else if (isNumericMeasure)
                {
                    var nums = grp
                        .Where(p => decimal.TryParse(p.MeasureValue, CultureInfo.InvariantCulture, out _))
                        .Select(p =>
                        {
                            var val = decimal.Parse(p.MeasureValue!, CultureInfo.InvariantCulture);
                            if (rule.Aggregation == AnalyticsAggregation.SignedSum &&
                                negativeValueSet is not null &&
                                p.SignValue is not null &&
                                negativeValueSet.Contains(p.SignValue))
                            {
                                val = -val;
                            }
                            return val;
                        })
                        .ToList();

                    if (nums.Count == 0) continue;

                    aggregatedValue = rule.Aggregation switch
                    {
                        AnalyticsAggregation.Sum => nums.Sum(),
                        AnalyticsAggregation.SignedSum => nums.Sum(),
                        AnalyticsAggregation.Avg => nums.Average(),
                        AnalyticsAggregation.Min => nums.Min(),
                        AnalyticsAggregation.Max => nums.Max(),
                        _ => nums.Sum()
                    };
                }
                else
                {
                    // Non-numeric measure with a non-Count aggregation: fall back to count
                    aggregatedValue = grp.Count();
                }

                groups.Add(new CrossFieldGroup(grp.Key!, aggregatedValue));
            }
            } // end else (non-CountByValue)

            if (groups.Count == 0) continue;

            var label = rule.Label
                ?? $"{rule.Aggregation} of {measureField.Name} by {groupByField.Name}";

            var unit = isNumericMeasure && rule.Aggregation is not AnalyticsAggregation.Count and not AnalyticsAggregation.CountByValue
                ? measureField.Unit
                : null;

            var filterDescription = rule.FilterFieldId.HasValue
                ? $"{rule.FilterFieldName} = {rule.FilterValue}"
                : null;

            var signDescription = rule.Aggregation == AnalyticsAggregation.SignedSum && rule.SignFieldName is not null
                ? $"{rule.SignFieldName} → −{rule.NegativeValues}"
                : null;

            // For CountByValue, SignFieldId is the optional value field
            string? valueFieldName = null;
            string? valueFieldUnit = null;
            HashSet<string> valueMetrics = [];
            if (rule.Aggregation == AnalyticsAggregation.CountByValue && rule.SignFieldId.HasValue)
            {
                if (fieldMap.TryGetValue(rule.SignFieldId.Value, out var valField))
                {
                    valueFieldName = valField.Name;
                    valueFieldUnit = valField.Unit is not null && valField.Unit != "UN" ? valField.Unit : null;
                }
                valueMetrics = DropdownValuesHelper.Split(rule.NegativeValues)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            // Build time-series data when DisplayType is TimeSeries
            var timeSeriesData = new List<CrossFieldTimeSeries>();
            if (rule.DisplayType == AnalyticsDisplayType.TimeSeries)
            {
                timeSeriesData = BuildCrossFieldTimeSeries(pairs, rule.Aggregation, isNumericMeasure, negativeValueSet);
            }

            results.Add(new CrossFieldResult
            {
                RuleId = rule.Id,
                Label = label,
                MeasureFieldName = measureField.Name,
                GroupByFieldName = groupByField.Name,
                Aggregation = rule.Aggregation,
                DisplayType = rule.DisplayType,
                Unit = unit,
                FilterDescription = filterDescription,
                SignDescription = signDescription,
                IsDropdownMeasure = isDropdownMeasure,
                ValueFieldName = valueFieldName,
                ValueFieldUnit = valueFieldUnit,
                ValueMetrics = valueMetrics,
                Groups = groups,
                TimeSeriesData = timeSeriesData
            });
        }

        return results;
    }

    /// <summary>Build time-series for cross-field analytics: one line per group-by value, aggregated per day.</summary>
    private static List<CrossFieldTimeSeries> BuildCrossFieldTimeSeries(
        List<(DateTime Date, string? GroupKey, string? MeasureValue, string? SignValue)> pairs,
        AnalyticsAggregation aggregation,
        bool isNumericMeasure,
        HashSet<string>? negativeValueSet)
    {
        return pairs
            .Where(p => !string.IsNullOrWhiteSpace(p.GroupKey))
            .GroupBy(p => p.GroupKey!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(15)
            .Select(grp =>
            {
                List<NumericDataPoint> points;

                if (aggregation == AnalyticsAggregation.Count || !isNumericMeasure)
                {
                    // Count entries per day for this group value
                    points = grp
                        .GroupBy(p => p.Date.Date)
                        .OrderBy(dg => dg.Key)
                        .Select(dg => new NumericDataPoint(dg.Key, dg.Count()))
                        .ToList();
                }
                else
                {
                    // Aggregate numeric values per day
                    points = grp
                        .Where(p => decimal.TryParse(p.MeasureValue, CultureInfo.InvariantCulture, out _))
                        .GroupBy(p => p.Date.Date)
                        .OrderBy(dg => dg.Key)
                        .Select(dg =>
                        {
                            var nums = dg.Select(p =>
                            {
                                var val = decimal.Parse(p.MeasureValue!, CultureInfo.InvariantCulture);
                                if (aggregation == AnalyticsAggregation.SignedSum &&
                                    negativeValueSet is not null &&
                                    p.SignValue is not null &&
                                    negativeValueSet.Contains(p.SignValue))
                                {
                                    val = -val;
                                }
                                return val;
                            }).ToList();

                            var value = aggregation switch
                            {
                                AnalyticsAggregation.Sum or AnalyticsAggregation.SignedSum => nums.Sum(),
                                AnalyticsAggregation.Avg => nums.Average(),
                                AnalyticsAggregation.Min => nums.Min(),
                                AnalyticsAggregation.Max => nums.Max(),
                                _ => nums.Sum()
                            };

                            return new NumericDataPoint(dg.Key, value);
                        })
                        .ToList();
                }

                return new CrossFieldTimeSeries
                {
                    SeriesLabel = grp.Key,
                    Points = points
                };
            })
            .Where(s => s.Points.Count > 0)
            .ToList();
    }

    private static List<RunningBalanceSeries> BuildRunningBalances(
        List<FieldAnalyticsRuleResponse> rules,
        List<ActionEntryResponse> entries,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        var balances = new List<RunningBalanceSeries>();

        // Only process SignedSum rules
        foreach (var rule in rules.Where(r => r.Aggregation == AnalyticsAggregation.SignedSum))
        {
            if (!fieldMap.TryGetValue(rule.MeasureFieldId, out var measureField))
                continue;

            var isNumeric = measureField.FieldType is FieldType.Integer or FieldType.Decimal;
            if (!isNumeric) continue;

            HashSet<string>? negativeValueSet = null;
            if (rule.NegativeValues is not null)
            {
                negativeValueSet = DropdownValuesHelper.Split(rule.NegativeValues)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            // Build signed values per entry, ordered chronologically
            var signedEntries = entries
                .OrderBy(e => e.OccurredAtUtc)
                .Select(e =>
                {
                    var measureVal = e.FieldValues
                        .FirstOrDefault(fv => fv.ActionFieldId == rule.MeasureFieldId)?.Value;
                    var signVal = rule.SignFieldId.HasValue
                        ? e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == rule.SignFieldId.Value)?.Value
                        : null;
                    return (Date: e.OccurredAtUtc, MeasureValue: measureVal, SignValue: signVal);
                })
                .Where(x => decimal.TryParse(x.MeasureValue, CultureInfo.InvariantCulture, out _))
                .ToList();

            if (signedEntries.Count == 0) continue;

            var points = new List<NumericDataPoint>();
            decimal runningTotal = 0;

            foreach (var entry in signedEntries)
            {
                var val = decimal.Parse(entry.MeasureValue!, CultureInfo.InvariantCulture);
                if (negativeValueSet is not null && entry.SignValue is not null &&
                    negativeValueSet.Contains(entry.SignValue))
                {
                    val = -val;
                }
                runningTotal += val;
                points.Add(new NumericDataPoint(entry.Date, runningTotal));
            }

            var unit = measureField.Unit is not null && measureField.Unit != "UN"
                ? measureField.Unit : "";

            var label = rule.Label ?? measureField.Name;

            var goalValue = measureField.TargetValueMode == TargetValueMode.Goal
                ? measureField.TargetValue
                : null;

            balances.Add(new RunningBalanceSeries
            {
                Label = label,
                Unit = unit,
                CurrentBalance = runningTotal,
                GoalValue = goalValue,
                MeasureFieldId = rule.MeasureFieldId,
                Points = points
            });
        }

        return balances;
    }

    private static List<BalanceSummary> BuildBalanceSummaries(
        List<FieldAnalyticsRuleResponse> signedSumRules,
        List<ActionEntryResponse> entries,
        Dictionary<Guid, ActionFieldResponse> fieldMap)
    {
        var summaries = new List<BalanceSummary>();

        foreach (var rule in signedSumRules)
        {
            if (!fieldMap.TryGetValue(rule.MeasureFieldId, out var measureField))
                continue;
            if (measureField.FieldType is not (FieldType.Integer or FieldType.Decimal))
                continue;

            HashSet<string>? negativeValueSet = null;
            if (rule.NegativeValues is not null)
            {
                negativeValueSet = DropdownValuesHelper.Split(rule.NegativeValues)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            decimal total = 0;
            foreach (var e in entries)
            {
                var measureVal = e.FieldValues
                    .FirstOrDefault(fv => fv.ActionFieldId == rule.MeasureFieldId)?.Value;
                if (!decimal.TryParse(measureVal, CultureInfo.InvariantCulture, out var val)) continue;

                var signVal = rule.SignFieldId.HasValue
                    ? e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == rule.SignFieldId.Value)?.Value
                    : null;

                if (negativeValueSet is not null && signVal is not null &&
                    negativeValueSet.Contains(signVal))
                    val = -val;

                total += val;
            }

            var unit = measureField.Unit is not null && measureField.Unit != "UN"
                ? measureField.Unit : "";

            summaries.Add(new BalanceSummary
            {
                Label = rule.Label ?? measureField.Name,
                Unit = unit,
                CurrentBalance = total,
                GoalValue = measureField.TargetValueMode == TargetValueMode.Goal ? measureField.TargetValue : null,
                MeasureFieldId = rule.MeasureFieldId
            });
        }

        return summaries;
    }

    private static List<NumericDataPoint> AggregateTrendPoints(
        List<NumericDataPoint> points, TrendAggregation mode)
    {
        if (mode == TrendAggregation.AllPoints || points.Count == 0)
            return points;

        return points
            .GroupBy(p => p.Date.Date)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var value = mode switch
                {
                    TrendAggregation.Average => g.Average(p => p.Value),
                    TrendAggregation.Min => g.Min(p => p.Value),
                    TrendAggregation.Max => g.Max(p => p.Value),
                    TrendAggregation.Sum => g.Sum(p => p.Value),
                    _ => g.Average(p => p.Value)
                };
                return new NumericDataPoint(g.Key, value);
            })
            .ToList();
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

            fieldStats.Add(BuildFieldStats(field, values, sorted));
        }

        // Build numeric time-series for inline charts
        var numericSeries = new List<NumericFieldSeries>();
        foreach (var field in fields.Where(f => f.FieldType is FieldType.Integer or FieldType.Decimal))
        {
            var rawPoints = entries
                .Select(e => (e.OccurredAtUtc, Val: e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == field.Id)?.Value))
                .Where(x => !string.IsNullOrWhiteSpace(x.Val) && decimal.TryParse(x.Val, CultureInfo.InvariantCulture, out _))
                .Select(x => new NumericDataPoint(x.OccurredAtUtc, decimal.Parse(x.Val!, CultureInfo.InvariantCulture)))
                .ToList();

            var points = AggregateTrendPoints(rawPoints, field.TrendAggregation);

            if (points.Count > 0)
            {
                numericSeries.Add(new NumericFieldSeries
                {
                    Name = field.Name,
                    Unit = field.Unit,
                    IsInteger = field.FieldType == FieldType.Integer,
                    ChartType = field.TrendChartType,
                    TargetValue = field.TargetValue,
                    TargetValueMode = field.TargetValueMode,
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

    private static FieldStats BuildFieldStats(ActionFieldResponse field, List<string> rawValues,
        List<ActionEntryResponse> sortedDescEntries)
    {
        var stats = new FieldStats
        {
            FieldId = field.Id,
            Name = field.Name,
            Type = field.FieldType,
            TotalValues = rawValues.Count,
            Unit = field.Unit,
            EnabledMetrics = field.SummaryMetrics,
            IsInteger = field.FieldType == FieldType.Integer
        };

        switch (field.FieldType)
        {
            case FieldType.Integer:
            case FieldType.Decimal:
                var numbers = rawValues
                    .Select(v => decimal.TryParse(v, CultureInfo.InvariantCulture, out var n) ? n : (decimal?)null)
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

                    // Latest value from the most recent entry
                    var latestValue = sortedDescEntries
                        .Select(e => e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == field.Id)?.Value)
                        .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                    if (latestValue is not null && decimal.TryParse(latestValue, CultureInfo.InvariantCulture, out var lat))
                        stats.NumericLatest = lat;
                }
                break;

            case FieldType.Boolean:
                var trueCount = rawValues.Count(v => v.Equals("True", StringComparison.OrdinalIgnoreCase));
                var falseCount = rawValues.Count - trueCount;
                stats.BoolTrueCount = trueCount;
                stats.BoolFalseCount = falseCount;
                break;

            case FieldType.Dropdown:
            case FieldType.CompositeDropdown:
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

    private static Dictionary<string, decimal> ComputeMetrics(List<decimal> values, HashSet<string> requested)
    {
        var metrics = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        if (requested.Contains("Sum")) metrics["Sum"] = values.Sum();
        if (requested.Contains("Avg")) metrics["Avg"] = values.Average();
        if (requested.Contains("Min")) metrics["Min"] = values.Min();
        if (requested.Contains("Max")) metrics["Max"] = values.Max();
        return metrics;
    }

    private static DropdownValueTrendSeries? BuildDropdownValueTrend(
        ActionFieldResponse dropdownField,
        ActionFieldResponse valueField,
        List<ActionEntryResponse> entries)
    {
        // For each entry, extract (date, dropdownValue, numericValue)
        var tuples = entries
            .Select(e =>
            {
                var ddVal = e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == dropdownField.Id)?.Value;
                var numVal = e.FieldValues.FirstOrDefault(fv => fv.ActionFieldId == valueField.Id)?.Value;
                return (Date: e.OccurredAtUtc, DropdownValue: ddVal, NumericValue: numVal);
            })
            .Where(t => !string.IsNullOrWhiteSpace(t.DropdownValue)
                     && decimal.TryParse(t.NumericValue, CultureInfo.InvariantCulture, out _))
            .Select(t => (t.Date, DropdownValue: t.DropdownValue!, NumericValue: decimal.Parse(t.NumericValue!, CultureInfo.InvariantCulture)))
            .ToList();

        if (tuples.Count == 0) return null;

        // Group by dropdown value, then build time series for each
        var seriesList = tuples
            .GroupBy(t => t.DropdownValue, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(15) // Limit to top 15 values for readability
            .Select(g =>
            {
                var rawPoints = g
                    .Select(t => new NumericDataPoint(t.Date, t.NumericValue))
                    .ToList();

                var aggregated = AggregateTrendPoints(rawPoints, dropdownField.DropdownTrendAggregation);

                return new DropdownValueSeries
                {
                    ValueLabel = g.Key,
                    Points = aggregated
                };
            })
            .Where(s => s.Points.Count > 0)
            .ToList();

        if (seriesList.Count == 0) return null;

        return new DropdownValueTrendSeries
        {
            FieldName = dropdownField.Name,
            ValueFieldName = valueField.Name,
            ValueFieldUnit = valueField.Unit,
            ChartType = dropdownField.DropdownTrendChartType,
            Series = seriesList
        };
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
    public List<BalanceSummary> Balances { get; set; } = [];
}

public sealed class BalanceSummary
{
    public required string Label { get; init; }
    public required string Unit { get; init; }
    public required decimal CurrentBalance { get; init; }
    public decimal? GoalValue { get; init; }
    public Guid? MeasureFieldId { get; init; }
}

public sealed record WeekBucket(DateTime WeekStart, int Count);

public sealed class FieldStats
{
    public Guid FieldId { get; init; }
    public required string Name { get; init; }
    public FieldType Type { get; init; }
    public int TotalValues { get; init; }
    public string Unit { get; init; } = "UN";
    public SummaryMetrics EnabledMetrics { get; init; } = SummaryMetrics.All;
    public bool IsInteger { get; init; }

    // Numeric (Integer/Decimal)
    public decimal? NumericMin { get; set; }
    public decimal? NumericMax { get; set; }
    public decimal? NumericAvg { get; set; }
    public decimal? NumericSum { get; set; }
    public decimal? NumericLatest { get; set; }
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
    public List<DropdownValueTrendSeries> DropdownValueTrends { get; init; } = [];
    public List<CrossFieldResult> CrossFieldResults { get; set; } = [];
    public List<RunningBalanceSeries> RunningBalances { get; set; } = [];
}

public sealed class RunningBalanceSeries
{
    public required string Label { get; init; }
    public required string Unit { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal? GoalValue { get; init; }
    public Guid? MeasureFieldId { get; init; }
    public List<NumericDataPoint> Points { get; init; } = [];
}

public sealed record DailyCount(DateTime Date, int Count);

public sealed class NumericFieldSeries
{
    public required string Name { get; init; }
    public required string Unit { get; init; }
    public bool IsInteger { get; init; }
    public TrendChartType ChartType { get; init; } = TrendChartType.Line;
    public decimal? TargetValue { get; init; }
    public TargetValueMode TargetValueMode { get; init; } = TargetValueMode.PerEntry;
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

/// <summary>Multi-series time trend chart for a dropdown field — one series per dropdown value.</summary>
public sealed class DropdownValueTrendSeries
{
    public required string FieldName { get; init; }
    public required string ValueFieldName { get; init; }
    public required string ValueFieldUnit { get; init; }
    public TrendChartType ChartType { get; init; } = TrendChartType.Line;
    public List<DropdownValueSeries> Series { get; init; } = [];
}

public sealed class DropdownValueSeries
{
    public required string ValueLabel { get; init; }
    public List<NumericDataPoint> Points { get; init; } = [];
}

// ── Cross-field analytics models ──

public sealed class CrossFieldResult
{
    public required Guid RuleId { get; init; }
    public required string Label { get; init; }
    public required string MeasureFieldName { get; init; }
    public required string GroupByFieldName { get; init; }
    public required AnalyticsAggregation Aggregation { get; init; }
    public required AnalyticsDisplayType DisplayType { get; init; }
    public string? Unit { get; init; }
    public string? FilterDescription { get; init; }
    public string? SignDescription { get; init; }
    public bool IsDropdownMeasure { get; init; }
    public string? ValueFieldName { get; init; }
    public string? ValueFieldUnit { get; init; }
    public HashSet<string> ValueMetrics { get; init; } = [];
    public List<CrossFieldGroup> Groups { get; init; } = [];
    public List<CrossFieldTimeSeries> TimeSeriesData { get; init; } = [];

    public decimal Total => Groups.Sum(g => g.Value);
}

public sealed class CrossFieldTimeSeries
{
    public required string SeriesLabel { get; init; }
    public List<NumericDataPoint> Points { get; init; } = [];
}

public sealed record CrossFieldGroup(string Key, decimal Value)
{
    /// <summary>Sub-distribution when measure is a Dropdown/CompositeDropdown (CountByValue).</summary>
    public List<CrossFieldGroup> SubGroups { get; init; } = [];

    /// <summary>Optional numeric metrics (Sum, Avg, Min, Max) when a value field is configured.</summary>
    public Dictionary<string, decimal>? Metrics { get; init; }
}
