namespace Traceon.Contracts.ConnectedActionRules;

public sealed record UpdateConnectedActionRuleRequest(
    string? Name = null,
    bool? IsEnabled = null,
    string? ConditionsJson = null,
    string? FieldMappingsJson = null,
    bool? CopyNotes = null,
    bool? CopyDate = null,
    int? SortOrder = null,
    bool ClearConditions = false,
    bool ClearMappings = false,
    bool? IsBidirectional = null);
