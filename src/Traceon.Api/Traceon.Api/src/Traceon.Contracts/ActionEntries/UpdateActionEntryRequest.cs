namespace Traceon.Contracts.ActionEntries;

public sealed record UpdateActionEntryRequest(
    DateTime OccurredAtUtc,
    string? Notes = null,
    List<ActionEntryFieldValue>? FieldValues = null);
