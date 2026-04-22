namespace Traceon.Domain.Entities;

public sealed class FieldAnalyticsRule : Entity
{
    public Guid TrackedActionId { get; private set; }
    public Guid MeasureFieldId { get; private set; }
    public Guid GroupByFieldId { get; private set; }
    public Guid? GroupByMetadataFieldId { get; private set; }
    public Guid? FilterFieldId { get; private set; }
    public Guid? FilterMetadataFieldId { get; private set; }
    public string? FilterValue { get; private set; }
    public int Aggregation { get; private set; }
    public int DisplayType { get; private set; }
    public string? Label { get; private set; }
    public int SortOrder { get; private set; }
    public Guid? SignFieldId { get; private set; }
    public string? NegativeValues { get; private set; }
    public Guid? OffsetTriggerFieldId { get; private set; }
    public string? OffsetTriggerValues { get; private set; }
    public Guid? OffsetValueFieldId { get; private set; }
    public int? OffsetDirection { get; private set; }
    public bool CollapseByImportBatch { get; private set; }

    private FieldAnalyticsRule(
        Guid trackedActionId,
        Guid measureFieldId,
        Guid groupByFieldId,
        Guid? groupByMetadataFieldId,
        Guid? filterFieldId,
        Guid? filterMetadataFieldId,
        string? filterValue,
        int aggregation,
        int displayType,
        string? label,
        int sortOrder,
        Guid? signFieldId,
        string? negativeValues,
        Guid? offsetTriggerFieldId,
        string? offsetTriggerValues,
        Guid? offsetValueFieldId,
        int? offsetDirection,
        bool collapseByImportBatch)
    {
        TrackedActionId = trackedActionId;
        MeasureFieldId = measureFieldId;
        GroupByFieldId = groupByFieldId;
        GroupByMetadataFieldId = groupByMetadataFieldId;
        FilterFieldId = filterFieldId;
        FilterMetadataFieldId = filterMetadataFieldId;
        FilterValue = filterValue;
        Aggregation = aggregation;
        DisplayType = displayType;
        Label = label;
        SortOrder = sortOrder;
        SignFieldId = signFieldId;
        NegativeValues = negativeValues;
        OffsetTriggerFieldId = offsetTriggerFieldId;
        OffsetTriggerValues = offsetTriggerValues;
        OffsetValueFieldId = offsetValueFieldId;
        OffsetDirection = offsetDirection;
        CollapseByImportBatch = collapseByImportBatch;
    }

    public static FieldAnalyticsRule Create(
        Guid trackedActionId,
        Guid measureFieldId,
        Guid groupByFieldId,
        int aggregation = 0,
        int displayType = 1,
        Guid? filterFieldId = null,
        string? filterValue = null,
        string? label = null,
        int sortOrder = 0,
        Guid? signFieldId = null,
        string? negativeValues = null,
        Guid? groupByMetadataFieldId = null,
        Guid? filterMetadataFieldId = null,
        Guid? offsetTriggerFieldId = null,
        string? offsetTriggerValues = null,
        Guid? offsetValueFieldId = null,
        int? offsetDirection = null,
        bool collapseByImportBatch = false)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        if (measureFieldId == Guid.Empty)
            throw new ArgumentException("Measure field ID is required.", nameof(measureFieldId));

        if (groupByFieldId == Guid.Empty)
            throw new ArgumentException("Group-by field ID is required.", nameof(groupByFieldId));

        if (measureFieldId == groupByFieldId && !groupByMetadataFieldId.HasValue)
            throw new ArgumentException("Measure and group-by fields must be different.", nameof(groupByFieldId));

        if (filterMetadataFieldId.HasValue && !filterFieldId.HasValue)
            throw new ArgumentException("Filter field must be set when a filter metadata field is specified.", nameof(filterMetadataFieldId));

        return new FieldAnalyticsRule(
            trackedActionId,
            measureFieldId,
            groupByFieldId,
            groupByMetadataFieldId,
            filterFieldId,
            filterMetadataFieldId,
            filterValue?.Trim(),
            aggregation,
            displayType,
            label?.Trim(),
            sortOrder,
            signFieldId,
            negativeValues?.Trim(),
            offsetTriggerFieldId,
            offsetTriggerValues?.Trim(),
            offsetValueFieldId,
            offsetDirection,
            collapseByImportBatch);
    }

    public void Update(
        int? aggregation = null,
        int? displayType = null,
        Guid? filterFieldId = null,
        string? filterValue = null,
        string? label = null,
        int? sortOrder = null,
        Guid? signFieldId = null,
        string? negativeValues = null,
        bool clearSignField = false,
        Guid? groupByMetadataFieldId = null,
        bool clearGroupByMetadataField = false,
        Guid? filterMetadataFieldId = null,
        bool clearFilterMetadataField = false,
        Guid? offsetTriggerFieldId = null,
        string? offsetTriggerValues = null,
        Guid? offsetValueFieldId = null,
        int? offsetDirection = null,
        bool clearOffset = false,
        bool? collapseByImportBatch = null)
    {
        if (aggregation.HasValue)
            Aggregation = aggregation.Value;
        if (displayType.HasValue)
            DisplayType = displayType.Value;
        FilterFieldId = filterFieldId;
        FilterValue = filterValue?.Trim();
        if (label is not null)
            Label = label.Trim();
        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;
        if (signFieldId.HasValue)
            SignFieldId = signFieldId;
        else if (clearSignField)
            SignFieldId = null;
        if (groupByMetadataFieldId.HasValue)
            GroupByMetadataFieldId = groupByMetadataFieldId;
        else if (clearGroupByMetadataField)
            GroupByMetadataFieldId = null;
        if (filterMetadataFieldId.HasValue)
            FilterMetadataFieldId = filterMetadataFieldId;
        else if (clearFilterMetadataField)
            FilterMetadataFieldId = null;
        NegativeValues = negativeValues?.Trim();
        if (clearOffset)
        {
            OffsetTriggerFieldId = null;
            OffsetTriggerValues = null;
            OffsetValueFieldId = null;
            OffsetDirection = null;
        }
        else
        {
            if (offsetTriggerFieldId.HasValue)
                OffsetTriggerFieldId = offsetTriggerFieldId;
            if (offsetTriggerValues is not null)
                OffsetTriggerValues = offsetTriggerValues.Trim();
            if (offsetValueFieldId.HasValue)
                OffsetValueFieldId = offsetValueFieldId;
            if (offsetDirection.HasValue)
                OffsetDirection = offsetDirection;
        }
        if (collapseByImportBatch.HasValue)
            CollapseByImportBatch = collapseByImportBatch.Value;
        MarkUpdated();
    }
}
