namespace Traceon.Contracts.ConnectedActionRules;

public sealed record CreateConnectedActionRuleRequest(
    Guid TargetTrackedActionId,
    string Name,
    bool IsEnabled = true,
    string? ConditionsJson = null,
    string? FieldMappingsJson = null,
    bool CopyNotes = true,
    bool CopyDate = true,
    int SortOrder = 0,
    bool IsBidirectional = false,
    string? ReverseExtraMappingsJson = null);
