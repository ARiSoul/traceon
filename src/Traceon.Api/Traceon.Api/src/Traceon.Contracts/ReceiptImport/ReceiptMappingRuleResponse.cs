namespace Traceon.Contracts.ReceiptImport;

public sealed record ReceiptMappingRuleResponse
{
    public required Guid Id { get; init; }
    public required Guid ReceiptImportConfigId { get; init; }
    public required Guid TargetFieldId { get; init; }
    public required string TargetFieldName { get; init; }
    public required string Pattern { get; init; }
    public required string Value { get; init; }
    public required int Priority { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
