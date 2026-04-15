using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldAnalyticsRules;

public sealed record FieldAnalyticsRuleResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required Guid MeasureFieldId { get; init; }
    public required string MeasureFieldName { get; init; }
    public required Guid GroupByFieldId { get; init; }
    public required string GroupByFieldName { get; init; }
    public required Guid? GroupByMetadataFieldId { get; init; }
    public required string? GroupByMetadataFieldName { get; init; }
    public required Guid? FilterFieldId { get; init; }
    public required string? FilterFieldName { get; init; }
    public required Guid? FilterMetadataFieldId { get; init; }
    public required string? FilterMetadataFieldName { get; init; }
    public required string? FilterValue { get; init; }
    public required AnalyticsAggregation Aggregation { get; init; }
    public required AnalyticsDisplayType DisplayType { get; init; }
    public required string? Label { get; init; }
    public required int SortOrder { get; init; }
    public required Guid? SignFieldId { get; init; }
    public required string? SignFieldName { get; init; }
    public required string? NegativeValues { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
