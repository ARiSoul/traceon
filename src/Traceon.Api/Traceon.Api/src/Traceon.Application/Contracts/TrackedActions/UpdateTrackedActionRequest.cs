namespace Traceon.Application.Contracts.TrackedActions;

public sealed record UpdateTrackedActionRequest(
    string Name,
    string? Description = null);
