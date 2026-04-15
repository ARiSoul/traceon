using Traceon.Contracts.Enums;

namespace Traceon.Domain.Entities;

public sealed class DropdownValueMetadataField : Entity
{
    public Guid FieldDefinitionId { get; private set; }
    public string Name { get; private set; }
    public FieldType Type { get; private set; }
    public string? Description { get; private set; }
    public bool IsRequired { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public string? DefaultValue { get; private set; }
    public string? Unit { get; private set; }
    public string? DropdownValues { get; private set; }
    public int SortOrder { get; private set; }
    public MetadataDisplayStyle DisplayStyle { get; private set; }

    private DropdownValueMetadataField(
        Guid fieldDefinitionId,
        string name,
        FieldType type,
        string? description,
        bool isRequired,
        decimal? minValue,
        decimal? maxValue,
        string? defaultValue,
        string? unit,
        string? dropdownValues,
        int sortOrder,
        MetadataDisplayStyle displayStyle)
    {
        FieldDefinitionId = fieldDefinitionId;
        Name = name;
        Type = type;
        Description = description;
        IsRequired = isRequired;
        MinValue = minValue;
        MaxValue = maxValue;
        DefaultValue = defaultValue;
        Unit = unit;
        DropdownValues = dropdownValues;
        SortOrder = sortOrder;
        DisplayStyle = displayStyle;
    }

    public static DropdownValueMetadataField Create(
        Guid fieldDefinitionId,
        string name,
        FieldType type,
        string? description = null,
        bool isRequired = false,
        decimal? minValue = null,
        decimal? maxValue = null,
        string? defaultValue = null,
        string? unit = null,
        string? dropdownValues = null,
        int sortOrder = 0,
        MetadataDisplayStyle displayStyle = MetadataDisplayStyle.Default)
    {
        if (fieldDefinitionId == Guid.Empty)
            throw new ArgumentException("Field definition ID is required.", nameof(fieldDefinitionId));

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (type == FieldType.CompositeDropdown)
            throw new ArgumentException("Composite dropdowns are not supported as metadata field types.", nameof(type));

        ValidateDisplayStyle(type, displayStyle, minValue, maxValue);

        return new DropdownValueMetadataField(
            fieldDefinitionId,
            name.Trim(),
            type,
            description?.Trim(),
            isRequired,
            minValue,
            maxValue,
            defaultValue?.Trim(),
            string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            dropdownValues?.Trim(),
            sortOrder,
            displayStyle);
    }

    public void Update(
        string name,
        FieldType type,
        string? description,
        bool isRequired,
        decimal? minValue,
        decimal? maxValue,
        string? defaultValue,
        string? unit,
        string? dropdownValues,
        MetadataDisplayStyle displayStyle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (type == FieldType.CompositeDropdown)
            throw new ArgumentException("Composite dropdowns are not supported as metadata field types.", nameof(type));

        ValidateDisplayStyle(type, displayStyle, minValue, maxValue);

        Name = name.Trim();
        Type = type;
        Description = description?.Trim();
        IsRequired = isRequired;
        MinValue = minValue;
        MaxValue = maxValue;
        DefaultValue = defaultValue?.Trim();
        Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
        DropdownValues = dropdownValues?.Trim();
        DisplayStyle = displayStyle;
        MarkUpdated();
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        MarkUpdated();
    }

    private static void ValidateDisplayStyle(FieldType type, MetadataDisplayStyle style, decimal? min, decimal? max)
    {
        if (style == MetadataDisplayStyle.Default) return;

        switch (style)
        {
            case MetadataDisplayStyle.Stars:
            case MetadataDisplayStyle.ProgressBar:
                if (type is not (FieldType.Integer or FieldType.Decimal))
                    throw new ArgumentException($"Display style '{style}' requires a numeric field type.", nameof(style));
                if (!min.HasValue || !max.HasValue)
                    throw new ArgumentException($"Display style '{style}' requires both Min and Max values.", nameof(style));
                if (max.Value <= min.Value)
                    throw new ArgumentException($"Display style '{style}' requires Max > Min.", nameof(style));
                break;

            case MetadataDisplayStyle.Badge:
                if (type is not (FieldType.Boolean or FieldType.Dropdown or FieldType.Text))
                    throw new ArgumentException("Badge display style only applies to Boolean, Dropdown, or Text fields.", nameof(style));
                break;
        }
    }
}
