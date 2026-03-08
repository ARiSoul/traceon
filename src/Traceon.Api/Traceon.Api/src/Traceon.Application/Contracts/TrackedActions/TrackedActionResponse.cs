namespace Traceon.Application.Contracts.TrackedActions;

public sealed record TrackedActionResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
