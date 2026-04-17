using Traceon.Contracts.Enums;

namespace Traceon.Contracts.CustomCharts;

public sealed record CreateCustomChartRequest(
    string Title,
    Guid MeasureFieldId,
    AnalyticsAggregation Aggregation = AnalyticsAggregation.Sum,
    CustomChartType ChartType = CustomChartType.Bar,
    Guid? GroupByFieldId = null,
    TimeGrouping TimeGrouping = TimeGrouping.None,
    FilterGroupDto? FilterConditions = null,
    string? ColorPalette = null,
    int SortOrder = 0,
    bool SortDescending = false,
    int? MaxGroups = null,
    bool ShowTotalizer = false);
