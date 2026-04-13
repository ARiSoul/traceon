namespace Traceon.Contracts.ConnectedActionRules;

public sealed record ConnectedActionRuleResponse
{
    public required Guid Id { get; init; }
    public required Guid SourceTrackedActionId { get; init; }
    public required string SourceTrackedActionName { get; init; }
    public required Guid TargetTrackedActionId { get; init; }
    public required string TargetTrackedActionName { get; init; }
    public required string Name { get; init; }
    public required bool IsEnabled { get; init; }
    public required string? ConditionsJson { get; init; }
    public required string? FieldMappingsJson { get; init; }
    public required bool CopyNotes { get; init; }
    public required bool CopyDate { get; init; }
    public required int SortOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
    public required Guid? PairedRuleId { get; init; }
}
