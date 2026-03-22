using Traceon.Contracts.TrackedActions;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class TrackedActionMappingExtensions
{
    public static TrackedActionResponse ToResponse(this TrackedAction entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Tags = [],
            FieldCount = entity.Fields.Count,
            EntryCount = 0,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };

    public static IReadOnlyList<TrackedActionResponse> ToResponseList(this IReadOnlyList<TrackedAction> entities) =>
        [.. entities.Select(e => e.ToResponse())];
}
