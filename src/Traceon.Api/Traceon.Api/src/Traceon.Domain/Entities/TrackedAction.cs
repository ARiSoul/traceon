namespace Traceon.Domain.Entities;

public sealed class TrackedAction : OwnedEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<ActionField> _fields = [];
    public IReadOnlyCollection<ActionField> Fields => _fields.AsReadOnly();

    private readonly List<TrackedActionTag> _tags = [];
    public IReadOnlyCollection<TrackedActionTag> Tags => _tags.AsReadOnly();

    private TrackedAction(string name, string? description, int sortOrder)
    {
        Name = name;
        Description = description;
        SortOrder = sortOrder;
    }

    public static TrackedAction Create(string userId, string name, string? description = null, int sortOrder = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var action = new TrackedAction(name.Trim(), description?.Trim(), sortOrder);
        action.SetOwner(userId);
        return action;
    }

    public void Update(string name, string? description, int? sortOrder = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
        if (sortOrder.HasValue)
            SortOrder = sortOrder.Value;
        MarkUpdated();
    }

    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        MarkUpdated();
    }

    public void AddTag(Tag tag)
    {
        if (_tags.Any(t => t.TagId == tag.Id))
            return;

        _tags.Add(TrackedActionTag.Create(Id, tag.Id));
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags.FirstOrDefault(t => t.TagId == tagId);

        if (tag is not null)
            _tags.Remove(tag);
    }
}
