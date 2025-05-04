namespace Arisoul.Traceon.Maui.Core.Entities;

public class FieldDefinition
    : BaseEntityWithId
{
    public string DefaultName { get; set; } = string.Empty;
    public string? DefaultDescription { get; set; }
    public FieldType Type { get; set; }
    public string? DropdownValues { get; set; } // comma-separated or JSON array

    public decimal? DefaultMaxValue { get; set; }
    public decimal? DefaultMinValue { get; set; }
    public bool DefaultIsRequired { get; set; }
}

public enum FieldType
{
    Text,
    Integer,
    Decimal,
    Date,
    Boolean,
    Dropdown
}
