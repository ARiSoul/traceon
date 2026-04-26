namespace Traceon.Contracts.ActionFields;

using Traceon.Contracts.Enums;

public sealed record UpdateActionFieldRequest(
    string Name,
    string? Description = null,
    decimal? MaxValue = null,
    decimal? MinValue = null,
    bool IsRequired = false,
    string? DefaultValue = null,
    string? Unit = null,
    int? Order = null,
    SummaryMetrics? SummaryMetrics = null,
    TrendAggregation? TrendAggregation = null,
    TrendChartType? TrendChartType = null,
    decimal? TargetValue = null,
    TargetValueMode? TargetValueMode = null,
    InitialValueBehavior? InitialValueBehavior = null,
    InitialValuePeriodUnit? InitialValuePeriodUnit = null,
    int? InitialValuePeriodCount = null,
    Guid? DropdownTrendValueFieldId = null,
    TrendAggregation? DropdownTrendAggregation = null,
    TrendChartType? DropdownTrendChartType = null,
    AutoCounterConfig? AutoCounterConfig = null,
    bool IsMultiselect = false,
    DisplayStyle DisplayStyle = DisplayStyle.Default,
    DisplayStyleConfig? DisplayStyleConfig = null);
