namespace Traceon.Domain.Entities;

public sealed class ConnectedActionRule : Entity
{
    public Guid SourceTrackedActionId { get; private set; }
    public Guid TargetTrackedActionId { get; private set; }
    public string Name { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? ConditionsJson { get; private set; }
    public string? FieldMappingsJson { get; private set; }
    public bool CopyNotes { get; private set; }
    public bool CopyDate { get; private set; }
    public int SortOrder { get; private set; }
    public Guid? PairedRuleId { get; private set; }

    private ConnectedActionRule(
        Guid sourceTrackedActionId,
        Guid targetTrackedActionId,
        string name,
        bool isEnabled,
        string? conditionsJson,
        string? fieldMappingsJson,
        bool copyNotes,
        bool copyDate,
        int sortOrder,
        Guid? pairedRuleId = null)
    {
        SourceTrackedActionId = sourceTrackedActionId;
        TargetTrackedActionId = targetTrackedActionId;
        Name = name;
        IsEnabled = isEnabled;
        ConditionsJson = conditionsJson;
        FieldMappingsJson = fieldMappingsJson;
        CopyNotes = copyNotes;
        CopyDate = copyDate;
        SortOrder = sortOrder;
        PairedRuleId = pairedRuleId;
    }

    public static ConnectedActionRule Create(
        Guid sourceTrackedActionId,
        Guid targetTrackedActionId,
        string name,
        bool isEnabled = true,
        string? conditionsJson = null,
        string? fieldMappingsJson = null,
        bool copyNotes = true,
        bool copyDate = true,
        int sortOrder = 0,
        Guid? pairedRuleId = null)
    {
        if (sourceTrackedActionId == Guid.Empty)
            throw new ArgumentException("Source tracked action ID is required.", nameof(sourceTrackedActionId));

        if (targetTrackedActionId == Guid.Empty)
            throw new ArgumentException("Target tracked action ID is required.", nameof(targetTrackedActionId));

        if (sourceTrackedActionId == targetTrackedActionId)
            throw new ArgumentException("Source and target actions must be different.", nameof(targetTrackedActionId));

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new ConnectedActionRule(
            sourceTrackedActionId,
            targetTrackedActionId,
            name.Trim(),
            isEnabled,
            conditionsJson?.Trim(),
            fieldMappingsJson?.Trim(),
            copyNotes,
            copyDate,
            sortOrder,
            pairedRuleId);
    }

    public void Update(
        string? name = null,
        bool? isEnabled = null,
        string? conditionsJson = null,
        string? fieldMappingsJson = null,
        bool? copyNotes = null,
        bool? copyDate = null,
        int? sortOrder = null,
        bool clearConditions = false,
        bool clearMappings = false)
    {
        if (name is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name.Trim();
        }

        if (isEnabled.HasValue)
            IsEnabled = isEnabled.Value;

        if (clearConditions)
            ConditionsJson = null;
        else if (conditionsJson is not null)
            ConditionsJson = conditionsJson.Trim();

        if (clearMappings)
            FieldMappingsJson = null;
        else if (fieldMappingsJson is not null)
            FieldMappingsJson = fieldMappingsJson.Trim();

        if (copyNotes.HasValue)
            CopyNotes = copyNotes.Value;

        if (copyDate.HasValue)
            CopyDate = copyDate.Value;

        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        MarkUpdated();
    }

    public void SetPairedRuleId(Guid pairedId)
    {
        PairedRuleId = pairedId;
        MarkUpdated();
    }

    public void ClearPairedRuleId()
    {
        PairedRuleId = null;
        MarkUpdated();
    }
}
