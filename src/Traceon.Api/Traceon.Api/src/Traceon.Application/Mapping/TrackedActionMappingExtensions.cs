using Traceon.Contracts.TrackedActions;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class TrackedActionMappingExtensions
{
    public static TrackedActionResponse ToResponse(this TrackedAction entity, bool hasReceiptConfig = false) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Tags = [],
            FieldCount = entity.Fields.Count,
            EntryCount = 0,
            SortOrder = entity.SortOrder,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            HasReceiptConfig = hasReceiptConfig
        };

    public static IReadOnlyList<TrackedActionResponse> ToResponseList(this IReadOnlyList<TrackedAction> entities, HashSet<Guid>? receiptEnabledIds = null) =>
        [.. entities.Select(e => e.ToResponse(receiptEnabledIds?.Contains(e.Id) ?? false))];
}
