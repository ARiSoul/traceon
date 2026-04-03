namespace Traceon.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    protected Entity()
    {
        Id = Guid.CreateVersion7();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void MarkUpdated() => UpdatedAtUtc = DateTime.UtcNow;

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
    }
}

public abstract class OwnedEntity : Entity
{
    public string UserId { get; private set; } = string.Empty;

    protected void SetOwner(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        UserId = userId;
    }
}
