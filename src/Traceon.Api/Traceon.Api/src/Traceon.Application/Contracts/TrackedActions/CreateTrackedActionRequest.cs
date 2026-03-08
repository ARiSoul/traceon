namespace Traceon.Application.Contracts.TrackedActions;

public sealed record CreateTrackedActionRequest(
    string Name,
    string? Description = null);
