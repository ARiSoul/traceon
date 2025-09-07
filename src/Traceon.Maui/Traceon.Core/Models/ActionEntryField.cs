namespace Arisoul.Traceon.Maui.Core.Models;

public class ActionEntryField
    : Entities.BaseEntityWithId
{
    public Guid ActionEntryId { get; set; }

    public ActionEntry ActionEntry { get; set; } = null!;

    public Guid ActionFieldId { get; set; }
    public ActionField ActionField { get; set; } = null!;

    public Guid FieldDefinitionId { get; set; }
    public FieldDefinition FieldDefinition { get; set; } = null!;

    public string? Value { get; set; } = string.Empty;
}
