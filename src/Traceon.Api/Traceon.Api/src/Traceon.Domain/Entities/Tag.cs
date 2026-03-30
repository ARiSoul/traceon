namespace Traceon.Domain.Entities;

public sealed class Tag : OwnedEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string Color { get; private set; }

    private Tag(string name, string? description, string color)
    {
        Name = name;
        Description = description;
        Color = color;
    }

    public static Tag Create(string userId, string name, string? description = null, string color = "#000000")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var tag = new Tag(name.Trim(), description?.Trim(), color.Trim());
        tag.SetOwner(userId);
        return tag;
    }

    public void Update(string name, string? description, string color)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
        Color = color.Trim();
        MarkUpdated();
    }
}
