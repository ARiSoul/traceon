namespace Traceon.Contracts.Tags;

public sealed record TagResponse(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
