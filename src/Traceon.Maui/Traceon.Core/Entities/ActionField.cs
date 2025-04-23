namespace Arisoul.Traceon.Maui.Core.Entities;

public class ActionField
{
    public Guid ActionId { get; set; }
    public TrackedAction Action { get; set; } = null!;

    public Guid FieldDefinitionId { get; set; }
    public FieldDefinition FieldDefinition { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? MinValue { get; set; }
    public bool IsRequired { get; set; }
}

