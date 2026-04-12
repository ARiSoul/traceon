namespace Traceon.Contracts.ActionEntries;

public sealed record BulkOperationResponse
{
    public required int AffectedCount { get; init; }
}
