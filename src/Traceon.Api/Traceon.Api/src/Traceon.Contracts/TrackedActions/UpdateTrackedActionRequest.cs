namespace Traceon.Contracts.TrackedActions;

public sealed record UpdateTrackedActionRequest(
    string Name,
    string? Description = null,
    int? SortOrder = null);