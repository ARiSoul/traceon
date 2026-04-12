namespace Traceon.Contracts.ActionEntries;

public sealed record BulkUpdateEntryFieldsRequest(
    List<Guid> EntryIds,
    List<ActionEntryFieldValue> FieldValues,
    DateTime? OccurredAtUtc = null,
    string? Notes = null,
    bool UpdateNotes = false);
