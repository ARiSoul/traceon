namespace Traceon.Contracts.TrackedActions;

public sealed record TrackedActionTagSummary
{
    public required string Name { get; init; }
    public required string Color { get; init; }
}
