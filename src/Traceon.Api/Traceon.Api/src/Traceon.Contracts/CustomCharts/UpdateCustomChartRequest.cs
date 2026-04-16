using Traceon.Contracts.Enums;

namespace Traceon.Contracts.CustomCharts;

public sealed record UpdateCustomChartRequest(
    string? Title = null,
    AnalyticsAggregation? Aggregation = null,
    CustomChartType? ChartType = null,
    Guid? GroupByFieldId = null,
    bool ClearGroupByField = false,
    TimeGrouping? TimeGrouping = null,
    FilterGroupDto? FilterConditions = null,
    bool ClearFilterConditions = false,
    string? ColorPalette = null,
    bool ClearColorPalette = false,
    int? SortOrder = null,
    bool? SortDescending = null,
    int? MaxGroups = null,
    bool ClearMaxGroups = false);
