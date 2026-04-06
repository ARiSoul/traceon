using Traceon.Contracts.Enums;
using Traceon.Contracts.FieldDependencyRules;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class FieldDependencyRuleMappingExtensions
{
    public static FieldDependencyRuleResponse ToResponse(
        this FieldDependencyRule entity,
        string sourceFieldName,
        string targetFieldName) =>
        new()
        {
            Id = entity.Id,
            TrackedActionId = entity.TrackedActionId,
            SourceFieldId = entity.SourceFieldId,
            SourceFieldName = sourceFieldName,
            SourceValue = entity.SourceValue,
            TargetFieldId = entity.TargetFieldId,
            TargetFieldName = targetFieldName,
            RuleType = (DependencyRuleType)entity.RuleType,
            TargetConstraint = entity.TargetConstraint,
            SortOrder = entity.SortOrder,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
