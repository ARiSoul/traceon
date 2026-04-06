using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldDependencyRules;

public sealed record FieldDependencyRuleResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required Guid SourceFieldId { get; init; }
    public required string SourceFieldName { get; init; }
    public required string? SourceValue { get; init; }
    public required Guid TargetFieldId { get; init; }
    public required string TargetFieldName { get; init; }
    public required DependencyRuleType RuleType { get; init; }
    public required string? TargetConstraint { get; init; }
    public required int SortOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
