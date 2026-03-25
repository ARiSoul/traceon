namespace Traceon.Domain.Entities;

public sealed class ActionField : Entity
{
    public Guid TrackedActionId { get; private set; }
    public Guid FieldDefinitionId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal? MaxValue { get; private set; }
    public decimal? MinValue { get; private set; }
    public bool IsRequired { get; private set; }
    public string? DefaultValue { get; private set; }
    public string Unit { get; private set; }
    public int Order { get; private set; }
    public int SummaryMetrics { get; private set; }
    public int TrendAggregation { get; private set; }
    public int TrendChartType { get; private set; }
    public decimal? TargetValue { get; private set; }

    private ActionField(
        Guid trackedActionId,
        Guid fieldDefinitionId,
        string name,
        string? description,
        decimal? maxValue,
        decimal? minValue,
        bool isRequired,
        string? defaultValue,
        string unit,
        int order,
        int summaryMetrics,
        int trendAggregation,
        int trendChartType,
        decimal? targetValue)
    {
        TrackedActionId = trackedActionId;
        FieldDefinitionId = fieldDefinitionId;
        Name = name;
        Description = description;
        MaxValue = maxValue;
        MinValue = minValue;
        IsRequired = isRequired;
        DefaultValue = defaultValue;
        Unit = unit;
        Order = order;
        SummaryMetrics = summaryMetrics;
        TrendAggregation = trendAggregation;
        TrendChartType = trendChartType;
        TargetValue = targetValue;
    }

    public static ActionField Create(
        Guid trackedActionId,
        Guid fieldDefinitionId,
        string name,
        string? description = null,
        decimal? maxValue = null,
        decimal? minValue = null,
        bool isRequired = false,
        string? defaultValue = null,
        string? unit = null,
        int order = 0,
        int summaryMetrics = 63,
        int trendAggregation = 0,
        int trendChartType = 0,
        decimal? targetValue = null)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        if (fieldDefinitionId == Guid.Empty)
            throw new ArgumentException("Field definition ID is required.", nameof(fieldDefinitionId));

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ActionField(
            trackedActionId,
            fieldDefinitionId,
            name.Trim(),
            description?.Trim(),
            maxValue,
            minValue,
            isRequired,
            defaultValue?.Trim(),
            string.IsNullOrWhiteSpace(unit) ? "UN" : unit.Trim(),
            order,
            summaryMetrics,
            trendAggregation,
            trendChartType,
            targetValue);
    }

    public void Update(
        string name,
        string? description = null,
        decimal? maxValue = null,
        decimal? minValue = null,
        bool isRequired = false,
        string? defaultValue = null,
        string? unit = null,
        int? order = null,
        int? summaryMetrics = null,
        int? trendAggregation = null,
        int? trendChartType = null,
        decimal? targetValue = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
        MaxValue = maxValue;
        MinValue = minValue;
        IsRequired = isRequired;
        DefaultValue = defaultValue?.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? "UN" : unit.Trim();
        if (order.HasValue)
            Order = order.Value;
        if (summaryMetrics.HasValue)
            SummaryMetrics = summaryMetrics.Value;
        if (trendAggregation.HasValue)
            TrendAggregation = trendAggregation.Value;
        if (trendChartType.HasValue)
            TrendChartType = trendChartType.Value;
        TargetValue = targetValue;
        MarkUpdated();
    }
}
