namespace Traceon.Contracts.ActionEntries;

public sealed record CreateActionEntryRequest(
    DateTime OccurredAtUtc,
    string? Notes = null,
    List<ActionEntryFieldValue>? FieldValues = null);

public sealed record ActionEntryFieldValue(
    Guid ActionFieldId,
    string? Value);
