using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldAnalyticsRules;

public sealed record UpdateFieldAnalyticsRuleRequest(
    AnalyticsAggregation? Aggregation = null,
    AnalyticsDisplayType? DisplayType = null,
    Guid? FilterFieldId = null,
    string? FilterValue = null,
    string? Label = null,
    int? SortOrder = null,
    Guid? SignFieldId = null,
    string? NegativeValues = null,
    bool ClearSignField = false,
    Guid? GroupByMetadataFieldId = null,
    bool ClearGroupByMetadataField = false,
    Guid? FilterMetadataFieldId = null,
    bool ClearFilterMetadataField = false,
    Guid? OffsetTriggerFieldId = null,
    string? OffsetTriggerValues = null,
    Guid? OffsetValueFieldId = null,
    AnalyticsOffsetDirection? OffsetDirection = null,
    bool ClearOffset = false,
    bool? CollapseByImportBatch = null);
