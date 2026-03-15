using Traceon.Contracts.ActionEntries;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ActionEntryMappingExtensions
{
    public static ActionEntryResponse ToResponse(this ActionEntry entity, IReadOnlyDictionary<Guid, string> fieldNames) =>
        new()
        {
            CreatedAtUtc = entity.CreatedAtUtc,
            FieldValues = [.. entity.Fields.Select(f => new ActionEntryFieldResponse
            {
                Id = f.Id,
                ActionFieldId = f.ActionFieldId,
                ActionFieldName = fieldNames.TryGetValue(f.ActionFieldId, out var fieldName) ? fieldName : string.Empty,
                Value = f.Value
            })],
            Id = entity.Id,
            OccurredAtUtc = entity.OccurredAtUtc,
            TrackedActionId = entity.TrackedActionId,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
