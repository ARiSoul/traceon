namespace Traceon.Contracts.TrackedActions;

public sealed record TrackedActionResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required List<TrackedActionTagSummary> Tags { get; init; }
    public required int FieldCount { get; init; }
    public required int EntryCount { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
