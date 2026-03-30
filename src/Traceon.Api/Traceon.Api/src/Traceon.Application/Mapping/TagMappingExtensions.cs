using Traceon.Contracts.Tags;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class TagMappingExtensions
{
    public static TagResponse ToResponse(this Tag entity) =>
        new()
        {
            Color = entity.Color,
            CreatedAtUtc = entity.CreatedAtUtc,
            Description = entity.Description,
            Id = entity.Id,
            Name = entity.Name,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    public static IReadOnlyList<TagResponse> ToResponseList(this IReadOnlyList<Tag> entities) =>
        [.. entities.Select(e => e.ToResponse())];
}
