namespace Traceon.Contracts.ActionFields;

using Traceon.Contracts.Enums;

public sealed record CreateActionFieldRequest(
    Guid FieldDefinitionId,
    string Name,
    string? Description = null,
    decimal? MaxValue = null,
    decimal? MinValue = null,
    bool IsRequired = false,
    string? DefaultValue = null,
    string? Unit = null,
    int Order = 0,
    SummaryMetrics SummaryMetrics = SummaryMetrics.All,
    TrendAggregation TrendAggregation = TrendAggregation.AllPoints,
    TrendChartType TrendChartType = TrendChartType.Line,
    decimal? TargetValue = null,
    TargetValueMode TargetValueMode = TargetValueMode.PerEntry,
    InitialValueBehavior InitialValueBehavior = InitialValueBehavior.DefaultValue,
    InitialValuePeriodUnit InitialValuePeriodUnit = InitialValuePeriodUnit.AllTime,
    int InitialValuePeriodCount = 0,
    Guid? DropdownTrendValueFieldId = null,
    TrendAggregation DropdownTrendAggregation = TrendAggregation.AllPoints,
    TrendChartType DropdownTrendChartType = TrendChartType.Line,
    AutoCounterConfig? AutoCounterConfig = null);
