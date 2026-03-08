namespace Traceon.Contracts.TrackedActions;

public sealed record CreateTrackedActionRequest(
    string Name,
    string? Description = null);
