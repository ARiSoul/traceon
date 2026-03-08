using Traceon.Contracts.Tags;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class TagMappingExtensions
{
    public static TagResponse ToResponse(this Tag entity) =>
        new(entity.Id,
            entity.Name,
            entity.Description,
            entity.Color,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);

    public static IReadOnlyList<TagResponse> ToResponseList(this IReadOnlyList<Tag> entities) =>
        entities.Select(e => e.ToResponse()).ToList();
}
