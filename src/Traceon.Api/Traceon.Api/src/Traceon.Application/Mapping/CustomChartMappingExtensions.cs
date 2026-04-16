using System.Text.Json;
using Traceon.Contracts.CustomCharts;
using Traceon.Contracts.Enums;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class CustomChartMappingExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static CustomChartResponse ToResponse(
        this CustomChart entity,
        string measureFieldName,
        string? groupByFieldName) =>
        new()
        {
            Id = entity.Id,
            TrackedActionId = entity.TrackedActionId,
            Title = entity.Title,
            MeasureFieldId = entity.MeasureFieldId,
            MeasureFieldName = measureFieldName,
            Aggregation = (AnalyticsAggregation)entity.Aggregation,
            ChartType = (CustomChartType)entity.ChartType,
            GroupByFieldId = entity.GroupByFieldId,
            GroupByFieldName = groupByFieldName,
            TimeGrouping = (TimeGrouping)entity.TimeGrouping,
            FilterConditions = DeserializeFilterConditions(entity.FilterConditionsJson),
            ColorPalette = entity.ColorPalette,
            SortOrder = entity.SortOrder,
            SortDescending = entity.SortDescending,
            MaxGroups = entity.MaxGroups,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    public static string? SerializeFilterConditions(FilterGroupDto? filterConditions)
    {
        if (filterConditions is null) return null;
        return JsonSerializer.Serialize(filterConditions, JsonOptions);
    }

    public static FilterGroupDto? DeserializeFilterConditions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<FilterGroupDto>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
