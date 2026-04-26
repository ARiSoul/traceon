namespace Traceon.Contracts.ActionEntries;

public sealed record AutoCounterPreviewRequest(
    Guid TargetActionFieldId,
    List<ActionEntryFieldInput>? FieldValues);

public sealed record AutoCounterPreviewResponse(
    Guid TargetActionFieldId,
    decimal? Value);
