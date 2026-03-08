using Traceon.Contracts.ActionEntries;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ActionEntryMappingExtensions
{
    public static ActionEntryResponse ToResponse(this ActionEntry entity, IReadOnlyDictionary<Guid, string> fieldNames) =>
        new(entity.Id,
            entity.TrackedActionId,
            entity.OccurredAtUtc,
            entity.Fields.Select(f => new ActionEntryFieldResponse(
                f.Id,
                f.ActionFieldId,
                fieldNames.GetValueOrDefault(f.ActionFieldId, string.Empty),
                f.Value)).ToList(),
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);
}
