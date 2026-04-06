namespace Traceon.Domain.Entities;

public sealed class FieldDependencyRule : Entity
{
    public Guid TrackedActionId { get; private set; }
    public Guid SourceFieldId { get; private set; }
    public string? SourceValue { get; private set; }
    public Guid TargetFieldId { get; private set; }
    public int RuleType { get; private set; }
    public string? TargetConstraint { get; private set; }
    public int SortOrder { get; private set; }

    private FieldDependencyRule(
        Guid trackedActionId,
        Guid sourceFieldId,
        string? sourceValue,
        Guid targetFieldId,
        int ruleType,
        string? targetConstraint,
        int sortOrder)
    {
        TrackedActionId = trackedActionId;
        SourceFieldId = sourceFieldId;
        SourceValue = sourceValue;
        TargetFieldId = targetFieldId;
        RuleType = ruleType;
        TargetConstraint = targetConstraint;
        SortOrder = sortOrder;
    }

    public static FieldDependencyRule Create(
        Guid trackedActionId,
        Guid sourceFieldId,
        Guid targetFieldId,
        string? sourceValue = null,
        int ruleType = 0,
        string? targetConstraint = null,
        int sortOrder = 0)
    {
        if (trackedActionId == Guid.Empty)
            throw new ArgumentException("Tracked action ID is required.", nameof(trackedActionId));

        if (sourceFieldId == Guid.Empty)
            throw new ArgumentException("Source field ID is required.", nameof(sourceFieldId));

        if (targetFieldId == Guid.Empty)
            throw new ArgumentException("Target field ID is required.", nameof(targetFieldId));

        if (sourceFieldId == targetFieldId)
            throw new ArgumentException("Source and target fields must be different.", nameof(targetFieldId));

        return new FieldDependencyRule(
            trackedActionId,
            sourceFieldId,
            sourceValue?.Trim(),
            targetFieldId,
            ruleType,
            targetConstraint?.Trim(),
            sortOrder);
    }

    public void Update(
        string? sourceValue = null,
        int? ruleType = null,
        string? targetConstraint = null,
        int? sortOrder = null,
        bool clearSourceValue = false)
    {
        if (clearSourceValue)
            SourceValue = null;
        else if (sourceValue is not null)
            SourceValue = sourceValue.Trim();

        if (ruleType.HasValue)
            RuleType = ruleType.Value;

        TargetConstraint = targetConstraint?.Trim();

        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;

        MarkUpdated();
    }
}
