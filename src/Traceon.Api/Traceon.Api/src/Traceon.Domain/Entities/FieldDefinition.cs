using Traceon.Contracts.Enums;

namespace Traceon.Domain.Entities;

public sealed class FieldDefinition : OwnedEntity
{
    public string DefaultName { get; private set; }
    public string? DefaultDescription { get; private set; }
    public FieldType Type { get; private set; }
    public string? DropdownValues { get; private set; }
    public decimal? DefaultMaxValue { get; private set; }
    public decimal? DefaultMinValue { get; private set; }
    public bool DefaultIsRequired { get; private set; }
    public string? DefaultValue { get; private set; }
    public string Unit { get; private set; }

    private FieldDefinition(
        string defaultName,
        string? defaultDescription,
        FieldType type,
        string? dropdownValues,
        decimal? defaultMaxValue,
        decimal? defaultMinValue,
        bool defaultIsRequired,
        string? defaultValue,
        string unit)
    {
        DefaultName = defaultName;
        DefaultDescription = defaultDescription;
        Type = type;
        DropdownValues = dropdownValues;
        DefaultMaxValue = defaultMaxValue;
        DefaultMinValue = defaultMinValue;
        DefaultIsRequired = defaultIsRequired;
        DefaultValue = defaultValue;
        Unit = unit;
    }

    public static FieldDefinition Create(
        string userId,
        string defaultName,
        FieldType type,
        string? defaultDescription = null,
        string? dropdownValues = null,
        decimal? defaultMaxValue = null,
        decimal? defaultMinValue = null,
        bool defaultIsRequired = false,
        string? defaultValue = null,
        string? unit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultName);

        var definition = new FieldDefinition(
            defaultName.Trim(),
            defaultDescription?.Trim(),
            type,
            dropdownValues?.Trim(),
            defaultMaxValue,
            defaultMinValue,
            defaultIsRequired,
            defaultValue?.Trim(),
            string.IsNullOrWhiteSpace(unit) ? "UN" : unit.Trim());

        definition.SetOwner(userId);
        return definition;
    }

    public void Update(
        string defaultName,
        FieldType type,
        string? defaultDescription = null,
        string? dropdownValues = null,
        decimal? defaultMaxValue = null,
        decimal? defaultMinValue = null,
        bool defaultIsRequired = false,
        string? defaultValue = null,
        string? unit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultName);

        DefaultName = defaultName.Trim();
        DefaultDescription = defaultDescription?.Trim();
        Type = type;
        DropdownValues = dropdownValues?.Trim();
        DefaultMaxValue = defaultMaxValue;
        DefaultMinValue = defaultMinValue;
        DefaultIsRequired = defaultIsRequired;
        DefaultValue = defaultValue?.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? "UN" : unit.Trim();
        MarkUpdated();
    }
}
