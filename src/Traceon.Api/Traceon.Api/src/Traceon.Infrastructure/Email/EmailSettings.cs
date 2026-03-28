namespace Traceon.Infrastructure.Email;

public sealed class EmailSettings
{
    public string SmtpHost { get; init; } = "localhost";
    public int SmtpPort { get; init; } = 587;
    public string? SmtpUser { get; init; }
    public string? SmtpPassword { get; init; }
    public bool UseSsl { get; init; } = true;
    public string FromEmail { get; init; } = "noreply@traceon.app";
    public string FromName { get; init; } = "Traceon";
    public string? ClientBaseUrl { get; init; }
}
