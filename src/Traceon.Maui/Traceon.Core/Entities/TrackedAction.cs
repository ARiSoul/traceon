namespace Arisoul.Traceon.Maui.Core.Entities;

public class TrackedAction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<CustomFieldDefinition> CustomFields { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ActionEntry> Entries { get; set; } = [];
}
