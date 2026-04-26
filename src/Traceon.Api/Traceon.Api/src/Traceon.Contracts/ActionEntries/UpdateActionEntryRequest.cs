namespace Traceon.Contracts.ActionEntries;

public sealed record UpdateActionEntryRequest(
    DateTime OccurredAtUtc,
    string? Notes = null,
    List<ActionEntryFieldInput>? FieldValues = null);
