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
        string? filterFieldName,
        string? signFieldName = null,
        string? groupByMetadataFieldName = null,
        string? filterMetadataFieldName = null,
        string? offsetTriggerFieldName = null,
        string? offsetValueFieldName = null) =>
        new()
        {
            Id = entity.Id,
            TrackedActionId = entity.TrackedActionId,
            MeasureFieldId = entity.MeasureFieldId,
            MeasureFieldName = measureFieldName,
            GroupByFieldId = entity.GroupByFieldId,
            GroupByFieldName = groupByFieldName,
            GroupByMetadataFieldId = entity.GroupByMetadataFieldId,
            GroupByMetadataFieldName = groupByMetadataFieldName,
            FilterFieldId = entity.FilterFieldId,
            FilterFieldName = filterFieldName,
            FilterMetadataFieldId = entity.FilterMetadataFieldId,
            FilterMetadataFieldName = filterMetadataFieldName,
            FilterValue = entity.FilterValue,
            Aggregation = (AnalyticsAggregation)entity.Aggregation,
            DisplayType = (AnalyticsDisplayType)entity.DisplayType,
            Label = entity.Label,
            SortOrder = entity.SortOrder,
            SignFieldId = entity.SignFieldId,
            SignFieldName = signFieldName,
            NegativeValues = entity.NegativeValues,
            OffsetTriggerFieldId = entity.OffsetTriggerFieldId,
            OffsetTriggerFieldName = offsetTriggerFieldName,
            OffsetTriggerValues = entity.OffsetTriggerValues,
            OffsetValueFieldId = entity.OffsetValueFieldId,
            OffsetValueFieldName = offsetValueFieldName,
            OffsetDirection = entity.OffsetDirection.HasValue
                ? (AnalyticsOffsetDirection)entity.OffsetDirection.Value
                : null,
            CollapseByImportBatch = entity.CollapseByImportBatch,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
