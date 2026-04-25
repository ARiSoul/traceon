namespace Traceon.Contracts.ActionEntries;

public sealed record AutoCounterPreviewRequest(
    Guid TargetActionFieldId,
    List<ActionEntryFieldValue>? FieldValues);

public sealed record AutoCounterPreviewResponse(
    Guid TargetActionFieldId,
    decimal? Value);
