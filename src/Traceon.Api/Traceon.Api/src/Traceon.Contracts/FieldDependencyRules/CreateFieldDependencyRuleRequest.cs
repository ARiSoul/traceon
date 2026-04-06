using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldDependencyRules;

public sealed record CreateFieldDependencyRuleRequest(
    Guid SourceFieldId,
    string? SourceValue,
    Guid TargetFieldId,
    DependencyRuleType RuleType = DependencyRuleType.FilterDropdownValues,
    string? TargetConstraint = null,
    int SortOrder = 0);
