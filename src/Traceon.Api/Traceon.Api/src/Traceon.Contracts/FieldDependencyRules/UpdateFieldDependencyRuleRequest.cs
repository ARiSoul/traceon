using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldDependencyRules;

public sealed record UpdateFieldDependencyRuleRequest(
    string? SourceValue = null,
    DependencyRuleType? RuleType = null,
    string? TargetConstraint = null,
    int? SortOrder = null);
