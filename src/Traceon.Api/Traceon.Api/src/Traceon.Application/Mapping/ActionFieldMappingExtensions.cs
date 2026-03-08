using Traceon.Contracts.ActionFields;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ActionFieldMappingExtensions
{
    public static ActionFieldResponse ToResponse(this ActionField entity, FieldDefinition fieldDefinition) =>
        new(entity.Id,
            entity.TrackedActionId,
            entity.FieldDefinitionId,
            fieldDefinition.Type,
            entity.Name,
            entity.Description,
            entity.MaxValue,
            entity.MinValue,
            entity.IsRequired,
            entity.DefaultValue,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);
}
