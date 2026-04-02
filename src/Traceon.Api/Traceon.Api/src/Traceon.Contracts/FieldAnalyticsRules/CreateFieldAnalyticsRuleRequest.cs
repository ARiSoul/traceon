using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldAnalyticsRules;

public sealed record CreateFieldAnalyticsRuleRequest(
    Guid MeasureFieldId,
    Guid GroupByFieldId,
    AnalyticsAggregation Aggregation = AnalyticsAggregation.Sum,
    AnalyticsDisplayType DisplayType = AnalyticsDisplayType.BarChart,
    Guid? FilterFieldId = null,
    string? FilterValue = null,
    string? Label = null,
    int SortOrder = 0);
