using Traceon.Contracts.Enums;

namespace Traceon.Contracts.ActionFields;

public sealed record ActionFieldResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required Guid FieldDefinitionId { get; init; }
    public required string FieldDefinitionName { get; init; }
    public required FieldType FieldType { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required decimal? MaxValue { get; init; }
    public required decimal? MinValue { get; init; }
    public required bool IsRequired { get; init; }
    public required string? DefaultValue { get; init; }
    public required string? DropdownValues { get; init; }
    public required string Unit { get; init; }
    public required int Order { get; init; }
    public required SummaryMetrics SummaryMetrics { get; init; }
    public required TrendAggregation TrendAggregation { get; init; }
    public required TrendChartType TrendChartType { get; init; }
    public required decimal? TargetValue { get; init; }
    public required InitialValueBehavior InitialValueBehavior { get; init; }
    public required InitialValuePeriodUnit InitialValuePeriodUnit { get; init; }
    public required int InitialValuePeriodCount { get; init; }
    public required Guid? DropdownTrendValueFieldId { get; init; }
    public required TrendAggregation DropdownTrendAggregation { get; init; }
    public required TrendChartType DropdownTrendChartType { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}