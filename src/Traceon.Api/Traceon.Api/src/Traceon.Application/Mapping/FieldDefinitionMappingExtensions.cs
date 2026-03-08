using Traceon.Contracts.FieldDefinitions;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class FieldDefinitionMappingExtensions
{
    public static FieldDefinitionResponse ToResponse(this FieldDefinition entity) =>
        new(entity.Id,
            entity.DefaultName,
            entity.DefaultDescription,
            entity.Type,
            entity.DropdownValues,
            entity.DefaultMaxValue,
            entity.DefaultMinValue,
            entity.DefaultIsRequired,
            entity.DefaultValue,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);

    public static IReadOnlyList<FieldDefinitionResponse> ToResponseList(this IReadOnlyList<FieldDefinition> entities) =>
        entities.Select(e => e.ToResponse()).ToList();
}
