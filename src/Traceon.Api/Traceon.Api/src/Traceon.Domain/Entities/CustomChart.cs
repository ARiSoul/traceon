namespace Traceon.Domain.Entities;

public sealed class CustomChart : Entity
{
    public Guid TrackedActionId { get; private set; }
    public string Title { get; private set; } = string.Empty;

    // Measure
    public Guid MeasureFieldId { get; private set; }
    public int Aggregation { get; private set; }

    // Grouping
    public Guid? GroupByFieldId { get; private set; }
    public int TimeGrouping { get; private set; }

    // Chart
    public int ChartType { get; private set; }
    public string? ColorPalette { get; private set; }

    // Filters (JSON-serialized composite filter tree)
    public string? FilterConditionsJson { get; private set; }

    // Ordering & display
    public int SortOrder { get; private set; }
    public bool SortDescending { get; private set; }
    public int? MaxGroups { get; private set; }

    private CustomChart(
        Guid trackedActionId,
        string title,
        Guid measureFieldId,
        int aggregation,
        Guid? groupByFieldId,
        int timeGrouping,
        int chartType,
        string? filterConditionsJson,
        string? colorPalette,
        int sortOrder,
        bool sortDescending,
        int? maxGroups)
    {
        TrackedActionId = trackedActionId;
        Title = title;
        MeasureFieldId = measureFieldId;
        Aggregation = aggregation;
        GroupByFieldId = groupByFieldId;
        TimeGrouping = timeGrouping;
        ChartType = chartType;
        FilterConditionsJson = filterConditionsJson;
        ColorPalette = colorPalette;
        SortOrder = sortOrder;
        SortDescending = sortDescending;
        MaxGroups = maxGroups;
    }

    public static CustomChart Create(
        Guid trackedActionId,
        string title,
        Guid measureFieldId,
        int aggregation = 0,
        int chartType = 0,
        Guid? groupByFieldId = null,
        int timeGrouping = 0,
        string? filterConditionsJson = null,
        string? colorPalette = null,
        int sortOrder = 0,
        bool sortDescending = false,
        int? maxGroups = null)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (measureFieldId == Guid.Empty)
            throw new ArgumentException("Measure field ID is required.", nameof(measureFieldId));

        return new CustomChart(
            trackedActionId,
            title.Trim(),
            measureFieldId,
            aggregation,
            groupByFieldId,
            timeGrouping,
            chartType,
            filterConditionsJson,
            colorPalette?.Trim(),
            sortOrder,
            sortDescending,
            maxGroups);
    }

    public void Update(
        string? title = null,
        int? aggregation = null,
        int? chartType = null,
        Guid? groupByFieldId = null,
        bool clearGroupByField = false,
        int? timeGrouping = null,
        string? filterConditionsJson = null,
        bool clearFilterConditions = false,
        string? colorPalette = null,
        bool clearColorPalette = false,
        int? sortOrder = null,
        bool? sortDescending = null,
        int? maxGroups = null,
        bool clearMaxGroups = false)
    {
        if (title is not null)
            Title = title.Trim();
        if (aggregation.HasValue)
            Aggregation = aggregation.Value;
        if (chartType.HasValue)
            ChartType = chartType.Value;
        if (groupByFieldId.HasValue)
            GroupByFieldId = groupByFieldId;
        else if (clearGroupByField)
            GroupByFieldId = null;
        if (timeGrouping.HasValue)
            TimeGrouping = timeGrouping.Value;
        if (filterConditionsJson is not null)
            FilterConditionsJson = filterConditionsJson;
        else if (clearFilterConditions)
            FilterConditionsJson = null;
        if (colorPalette is not null)
            ColorPalette = colorPalette.Trim();
        else if (clearColorPalette)
            ColorPalette = null;
        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;
        if (sortDescending.HasValue)
            SortDescending = sortDescending.Value;
        if (maxGroups.HasValue)
            MaxGroups = maxGroups.Value;
        else if (clearMaxGroups)
            MaxGroups = null;
        MarkUpdated();
    }
}
