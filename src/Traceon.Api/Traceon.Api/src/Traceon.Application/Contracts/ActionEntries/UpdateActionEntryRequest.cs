namespace Traceon.Application.Contracts.ActionEntries;

public sealed record UpdateActionEntryRequest(
    DateTime OccurredAtUtc,
    List<ActionEntryFieldValue>? FieldValues = null);
