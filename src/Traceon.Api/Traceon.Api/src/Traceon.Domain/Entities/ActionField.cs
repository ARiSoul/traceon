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

    private ActionField(
        Guid trackedActionId,
        Guid fieldDefinitionId,
        string name,
        string? description,
        decimal? maxValue,
        decimal? minValue,
        bool isRequired,
        string? defaultValue,
        string unit)
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
        string? unit = null)
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
            string.IsNullOrWhiteSpace(unit) ? "UN" : unit.Trim());
    }

    public void Update(
        string name,
        string? description = null,
        decimal? maxValue = null,
        decimal? minValue = null,
        bool isRequired = false,
        string? defaultValue = null,
        string? unit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
        MaxValue = maxValue;
        MinValue = minValue;
        IsRequired = isRequired;
        DefaultValue = defaultValue?.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? "UN" : unit.Trim();
        MarkUpdated();
    }
}
