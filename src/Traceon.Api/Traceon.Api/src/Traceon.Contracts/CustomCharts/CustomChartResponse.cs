using Traceon.Contracts.Enums;

namespace Traceon.Contracts.CustomCharts;

public sealed record CustomChartResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required string Title { get; init; }
    public required Guid MeasureFieldId { get; init; }
    public required string MeasureFieldName { get; init; }
    public required AnalyticsAggregation Aggregation { get; init; }
    public required CustomChartType ChartType { get; init; }
    public required Guid? GroupByFieldId { get; init; }
    public required string? GroupByFieldName { get; init; }
    public required TimeGrouping TimeGrouping { get; init; }
    public required FilterGroupDto? FilterConditions { get; init; }
    public required string? ColorPalette { get; init; }
    public required int SortOrder { get; init; }
    public required bool SortDescending { get; init; }
    public required int? MaxGroups { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
