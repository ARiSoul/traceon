namespace Traceon.Infrastructure.GitHub;

public sealed class GitHubSettings
{
    public string? Token { get; init; }
    public string Owner { get; init; } = "ARiSoul";
    public string Repo { get; init; } = "traceon";
}
