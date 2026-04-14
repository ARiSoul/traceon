using Traceon.Contracts.ConnectedActionRules;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ConnectedActionRuleMappingExtensions
{
    public static ConnectedActionRuleResponse ToResponse(
        this ConnectedActionRule entity,
        string sourceActionName,
        string targetActionName) =>
        new()
        {
            Id = entity.Id,
            SourceTrackedActionId = entity.SourceTrackedActionId,
            SourceTrackedActionName = sourceActionName,
            TargetTrackedActionId = entity.TargetTrackedActionId,
            TargetTrackedActionName = targetActionName,
            Name = entity.Name,
            IsEnabled = entity.IsEnabled,
            ConditionsJson = entity.ConditionsJson,
            FieldMappingsJson = entity.FieldMappingsJson,
            CopyNotes = entity.CopyNotes,
            CopyDate = entity.CopyDate,
            SortOrder = entity.SortOrder,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            PairedRuleId = entity.PairedRuleId
        };
}
