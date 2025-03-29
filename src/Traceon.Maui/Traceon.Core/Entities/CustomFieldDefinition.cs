namespace Arisoul.Traceon.Maui.Core.Entities;

public class CustomFieldDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public CustomFieldType Type { get; set; }
    public bool Required { get; set; }
}

public enum CustomFieldType
{
    Text,
    Number,
    Date,
    Boolean,
    Dropdown
}
