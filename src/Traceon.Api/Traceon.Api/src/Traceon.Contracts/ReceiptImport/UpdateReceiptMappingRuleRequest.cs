namespace Traceon.Contracts.ReceiptImport;

public sealed record UpdateReceiptMappingRuleRequest(
    string? Pattern = null,
    string? Value = null,
    int? Priority = null);
