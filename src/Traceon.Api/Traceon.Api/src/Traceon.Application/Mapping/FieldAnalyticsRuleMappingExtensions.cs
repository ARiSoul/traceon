using Traceon.Contracts.Enums;
using Traceon.Contracts.FieldAnalyticsRules;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class FieldAnalyticsRuleMappingExtensions
{
    public static FieldAnalyticsRuleResponse ToResponse(
        this FieldAnalyticsRule entity,
        string measureFieldName,
        string groupByFieldName,
        string? filterFieldName) =>
        new()
        {
            Id = entity.Id,
            TrackedActionId = entity.TrackedActionId,
            MeasureFieldId = entity.MeasureFieldId,
            MeasureFieldName = measureFieldName,
            GroupByFieldId = entity.GroupByFieldId,
            GroupByFieldName = groupByFieldName,
            FilterFieldId = entity.FilterFieldId,
            FilterFieldName = filterFieldName,
            FilterValue = entity.FilterValue,
            Aggregation = (AnalyticsAggregation)entity.Aggregation,
            DisplayType = (AnalyticsDisplayType)entity.DisplayType,
            Label = entity.Label,
            SortOrder = entity.SortOrder,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
