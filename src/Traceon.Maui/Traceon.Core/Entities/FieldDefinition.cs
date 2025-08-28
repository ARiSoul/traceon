using System.ComponentModel.DataAnnotations;
using Arisoul.Traceon.Localization;

namespace Arisoul.Traceon.Maui.Core.Entities;

public class FieldDefinition
    : BaseEntityWithId
{
    public string DefaultName { get; set; } = string.Empty;
    public string? DefaultDescription { get; set; }
    public FieldType Type { get; set; }
    public string? DropdownValues { get; set; } // comma-separated

    public decimal? DefaultMaxValue { get; set; }
    public decimal? DefaultMinValue { get; set; }
    public bool DefaultIsRequired { get; set; }
}

public enum FieldType
{
    [Display(Name = nameof(Strings.FieldType_Text), ResourceType = typeof(Strings))]
    Text,
    
    [Display(Name = nameof(Strings.FieldType_Integer), ResourceType = typeof(Strings))]
    Integer,
    
    [Display(Name = nameof(Strings.FieldType_Decimal), ResourceType = typeof(Strings))]
    Decimal,
    
    [Display(Name = nameof(Strings.FieldType_Date), ResourceType = typeof(Strings))]
    Date,
    
    [Display(Name = nameof(Strings.FieldType_Boolean), ResourceType = typeof(Strings))]
    Boolean,

    [Display(Name = nameof(Strings.FieldType_Dropdown), ResourceType = typeof(Strings))]
    Dropdown
}
