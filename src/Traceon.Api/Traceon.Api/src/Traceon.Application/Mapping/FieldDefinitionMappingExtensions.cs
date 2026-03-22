using Traceon.Contracts.FieldDefinitions;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class FieldDefinitionMappingExtensions
{
    public static FieldDefinitionResponse ToResponse(this FieldDefinition entity) =>
        new()
        {
            CreatedAtUtc = entity.CreatedAtUtc,
            DefaultDescription = entity.DefaultDescription,
            DefaultIsRequired = entity.DefaultIsRequired,
            DefaultMaxValue = entity.DefaultMaxValue,
            DefaultMinValue = entity.DefaultMinValue,
            DefaultName = entity.DefaultName,
            DefaultValue = entity.DefaultValue,
            DropdownValues = entity.DropdownValues,
            Id = entity.Id,
            Type = entity.Type,
            Unit = entity.Unit,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    public static IReadOnlyList<FieldDefinitionResponse> ToResponseList(this IReadOnlyList<FieldDefinition> entities) =>
        [.. entities.Select(e => e.ToResponse())];
}
