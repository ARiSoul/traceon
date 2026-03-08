namespace Traceon.Contracts.ActionEntries;

public sealed record UpdateActionEntryRequest(
    DateTime OccurredAtUtc,
    List<ActionEntryFieldValue>? FieldValues = null);
