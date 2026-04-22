using Traceon.Contracts.ActionEntries;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class ActionEntryMappingExtensions
{
    public static ActionEntryResponse ToResponse(this ActionEntry entity, IReadOnlyDictionary<Guid, string> fieldNames, string actionName = "") =>
        new()
        {
            Id = entity.Id,
            TrackedActionId = entity.TrackedActionId,
            ActionName = actionName,
            OccurredAtUtc = entity.OccurredAtUtc,
            Notes = entity.Notes,
            ReceiptImportBatchId = entity.ReceiptImportBatchId,
            FieldValues = [.. entity.Fields.Select(f => new ActionEntryFieldResponse
            {
                Id = f.Id,
                ActionFieldId = f.ActionFieldId,
                ActionFieldName = fieldNames.TryGetValue(f.ActionFieldId, out var fieldName) ? fieldName : string.Empty,
                Value = f.Value
            })],
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
}
