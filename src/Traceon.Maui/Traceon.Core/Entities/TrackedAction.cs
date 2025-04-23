namespace Arisoul.Traceon.Maui.Core.Entities;

public class TrackedAction
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<ActionField> Fields { get; set; } = [];
    public List<ActionTag> Tags { get; set; } = [];
    public List<ActionEntry> Entries { get; set; } = [];
}
