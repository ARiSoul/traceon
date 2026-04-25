using System.Text.Json;
using Traceon.Contracts.ActionFields;
using Traceon.Contracts.Enums;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ActionFieldMappingExtensions
{
    public static ActionFieldResponse ToResponse(this ActionField entity, FieldDefinition fieldDefinition) =>
        new()
        {
            CreatedAtUtc = entity.CreatedAtUtc,
            DefaultValue = entity.DefaultValue,
            Description = entity.Description,
            FieldDefinitionId = fieldDefinition.Id,
            FieldDefinitionName = fieldDefinition.DefaultName,
            FieldType = fieldDefinition.Type,
            Id = entity.Id,
            IsRequired = entity.IsRequired,
            MaxValue = entity.MaxValue,
            MinValue = entity.MinValue,
            Name = entity.Name,
            Order = entity.Order,
            SummaryMetrics = (SummaryMetrics)entity.SummaryMetrics,
            TrendAggregation = (TrendAggregation)entity.TrendAggregation,
            TrendChartType = (TrendChartType)entity.TrendChartType,
            TargetValue = entity.TargetValue,
            TargetValueMode = (TargetValueMode)entity.TargetValueMode,
            TrackedActionId = entity.TrackedActionId,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Unit = entity.Unit,
            DropdownValues = fieldDefinition.DropdownValues,
            InitialValueBehavior = (InitialValueBehavior)entity.InitialValueBehavior,
            InitialValuePeriodUnit = (InitialValuePeriodUnit)entity.InitialValuePeriodUnit,
            InitialValuePeriodCount = entity.InitialValuePeriodCount,
            DropdownTrendValueFieldId = entity.DropdownTrendValueFieldId,
            DropdownTrendAggregation = (TrendAggregation)entity.DropdownTrendAggregation,
            DropdownTrendChartType = (TrendChartType)entity.DropdownTrendChartType,
            AutoCounterConfig = DeserializeAutoCounter(entity.AutoCounterConfigJson)
        };

    public static string? SerializeAutoCounter(AutoCounterConfig? config)
        => config is null ? null : JsonSerializer.Serialize(config);

    public static AutoCounterConfig? DeserializeAutoCounter(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<AutoCounterConfig>(json); }
        catch { return null; }
    }
}
