namespace Traceon.Infrastructure.Identity;

public sealed class ExternalAuthSettings
{
    public GoogleSettings? Google { get; init; }
    public MicrosoftSettings? Microsoft { get; init; }
}

public sealed class GoogleSettings
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}

public sealed class MicrosoftSettings
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}
