namespace Arisoul.Traceon.Maui.Core.Models;

public class FieldDefinition
    : Entities.BaseEntityWithId
{
    public string DefaultName { get; set; } = string.Empty;
    public string? DefaultDescription { get; set; }
    public Entities.FieldType Type { get; set; }
    public string? DropdownValues { get; set; }

    public decimal? DefaultMaxValue { get; set; }
    public decimal? DefaultMinValue { get; set; }
    public bool DefaultIsRequired { get; set; }
}