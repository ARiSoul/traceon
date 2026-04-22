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
    int SortOrder = 0,
    Guid? SignFieldId = null,
    string? NegativeValues = null,
    Guid? GroupByMetadataFieldId = null,
    Guid? FilterMetadataFieldId = null,
    Guid? OffsetTriggerFieldId = null,
    string? OffsetTriggerValues = null,
    Guid? OffsetValueFieldId = null,
    AnalyticsOffsetDirection? OffsetDirection = null,
    bool CollapseByImportBatch = false);
