namespace Traceon.Contracts.ActionEntries;

public sealed record ActionEntryResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required string ActionName { get; init; }
    public required DateTime OccurredAtUtc { get; init; }
    public required string? Notes { get; init; }
    public required Guid? ReceiptImportBatchId { get; init; }
    public required List<ActionEntryFieldResponse> FieldValues { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}

public sealed record ActionEntryFieldResponse
{
    public required Guid Id { get; init; }
    public required Guid ActionFieldId { get; init; }
    public required string ActionFieldName { get; init; }
    public required List<string> Values { get; init; }

    public string? Value => Values is { Count: > 0 } ? Values[0] : null;
}
