using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Core.Entities;

public class ActionField
    : BaseActionChildEntity, IEntityWithId
{
    public Guid Id { get; set; }

    public Guid FieldDefinitionId { get; set; }
    public FieldDefinition FieldDefinition { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public bool IsRequired { get; set; }

    public bool IsIntegerType => FieldDefinition != null && FieldDefinition.Type == FieldType.Integer;
    public bool IsDecimalType => FieldDefinition != null && FieldDefinition.Type == FieldType.Decimal;
    public bool IsTextType => FieldDefinition != null && FieldDefinition.Type == FieldType.Text;
    public bool IsDateType => FieldDefinition != null && FieldDefinition.Type == FieldType.Date;
    public bool IsBooleanType => FieldDefinition != null && FieldDefinition.Type == FieldType.Boolean;
    public bool IsDropdownType => FieldDefinition != null && FieldDefinition.Type == FieldType.Dropdown;
    public List<string> DropdownValuesList => FieldDefinition != null && !string.IsNullOrWhiteSpace(FieldDefinition.DropdownValues)
        ? [.. FieldDefinition.DropdownValues.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]
        : [];
    public bool CanHaveMaxAndMinValues => IsIntegerType || IsDecimalType;
}

