namespace Traceon.Contracts.ActionEntries;

public sealed record CreateActionEntryRequest(
    DateTime OccurredAtUtc,
    string? Notes = null,
    List<ActionEntryFieldInput>? FieldValues = null,
    Guid? ReceiptImportBatchId = null);

public sealed record ActionEntryFieldInput(
    Guid ActionFieldId,
    List<string>? Values)
{
    public string? Value => Values is { Count: > 0 } ? Values[0] : null;
}
