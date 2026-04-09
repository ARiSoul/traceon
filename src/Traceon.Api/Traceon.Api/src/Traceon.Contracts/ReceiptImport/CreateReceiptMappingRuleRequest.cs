namespace Traceon.Contracts.ReceiptImport;

public sealed record CreateReceiptMappingRuleRequest(
    Guid TargetFieldId,
    string Pattern,
    string Value,
    int Priority = 0);
