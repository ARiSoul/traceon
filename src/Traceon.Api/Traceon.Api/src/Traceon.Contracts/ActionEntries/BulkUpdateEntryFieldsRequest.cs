namespace Traceon.Contracts.ActionEntries;

public sealed record BulkUpdateEntryFieldsRequest(
    List<Guid> EntryIds,
    List<ActionEntryFieldInput> FieldValues,
    DateTime? OccurredAtUtc = null,
    string? Notes = null,
    bool UpdateNotes = false);
