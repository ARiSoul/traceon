using System.Security.Cryptography;
using System.Text;

namespace Traceon.Infrastructure.Identity;

public sealed class UserRefreshToken
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private UserRefreshToken() { }

    public static UserRefreshToken Create(string userId, string token, int expirationDays)
    {
        return new UserRefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = HashToken(token),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
