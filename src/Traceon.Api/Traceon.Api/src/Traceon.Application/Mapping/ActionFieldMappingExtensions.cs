using Traceon.Contracts.ActionFields;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ActionFieldMappingExtensions
{
    public static ActionFieldResponse ToResponse(this ActionField entity, FieldDefinition fieldDefinition) =>
        new()
        {
            CreatedAtUtc = entity.CreatedAtUtc,
            DefaultValue = entity.DefaultValue,
            Description = entity.Description,
            FieldDefinitionId = fieldDefinition.Id,
            FieldType = fieldDefinition.Type,
            Id = entity.Id,
            IsRequired = entity.IsRequired,
            MaxValue = entity.MaxValue,
            MinValue = entity.MinValue,
            Name = entity.Name,
            Order = entity.Order,
            TrackedActionId = entity.TrackedActionId,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Unit = entity.Unit,
            DropdownValues = fieldDefinition.DropdownValues
        };
}
