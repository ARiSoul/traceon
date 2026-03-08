using Traceon.Application.Contracts.TrackedActions;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class TrackedActionMappingExtensions
{
    public static TrackedActionResponse ToResponse(this TrackedAction entity) =>
        new(entity.Id,
            entity.Name,
            entity.Description,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);

    public static IReadOnlyList<TrackedActionResponse> ToResponseList(this IReadOnlyList<TrackedAction> entities) =>
        entities.Select(e => e.ToResponse()).ToList();
}
