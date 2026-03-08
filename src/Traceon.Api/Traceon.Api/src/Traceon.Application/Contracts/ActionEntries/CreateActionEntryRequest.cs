namespace Traceon.Application.Contracts.ActionEntries;

public sealed record CreateActionEntryRequest(
    DateTime OccurredAtUtc,
    List<ActionEntryFieldValue>? FieldValues = null);

public sealed record ActionEntryFieldValue(
    Guid ActionFieldId,
    string? Value);
