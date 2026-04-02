using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldAnalyticsRules;

public sealed record UpdateFieldAnalyticsRuleRequest(
    AnalyticsAggregation? Aggregation = null,
    AnalyticsDisplayType? DisplayType = null,
    Guid? FilterFieldId = null,
    string? FilterValue = null,
    string? Label = null,
    int? SortOrder = null);
