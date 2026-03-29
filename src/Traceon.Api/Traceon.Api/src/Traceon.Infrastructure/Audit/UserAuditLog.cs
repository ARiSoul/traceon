namespace Traceon.Infrastructure.Audit;

public sealed class UserAuditLog
{
    public Guid Id { get; private set; }
    public string? UserId { get; private set; }
    public string UserEmail { get; private set; }
    public string Action { get; private set; }
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private UserAuditLog()
    {
        UserEmail = string.Empty;
        Action = string.Empty;
    }

    public static UserAuditLog Create(
        string? userId,
        string userEmail,
        string action,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new UserAuditLog
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}
