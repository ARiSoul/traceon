namespace Traceon.Contracts.ActionEntries;

public sealed record BulkDeleteEntriesRequest(
    List<Guid> EntryIds);
