namespace Traceon.Contracts.ActionEntries;

public sealed record ActionEntryResponse(
    Guid Id,
    Guid TrackedActionId,
    DateTime OccurredAtUtc,
    List<ActionEntryFieldResponse> FieldValues,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record ActionEntryFieldResponse(
    Guid Id,
    Guid ActionFieldId,
    string ActionFieldName,
    string? Value);
