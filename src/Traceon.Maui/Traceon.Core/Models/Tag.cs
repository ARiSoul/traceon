namespace Arisoul.Traceon.Maui.Core.Models;

public class Tag
    : Entities.BaseEntityWithId
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#000000";
}

