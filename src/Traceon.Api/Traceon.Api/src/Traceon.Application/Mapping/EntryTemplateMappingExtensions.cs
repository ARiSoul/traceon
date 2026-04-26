using Traceon.Contracts.EntryTemplates;
using Traceon.Domain.Entities;

namespace Traceon.Application.Mapping;

public static class EntryTemplateMappingExtensions
{
    public static EntryTemplateResponse ToResponse(this EntryTemplate entity) => new()
    {
        Id = entity.Id,
        TrackedActionId = entity.TrackedActionId,
        Name = entity.Name,
        Notes = entity.Notes,
        FieldValues = [.. entity.Fields.Select(f => new EntryTemplateFieldResponse
        {
            Id = f.Id,
            ActionFieldId = f.ActionFieldId,
            Values = [.. f.Values.OrderBy(v => v.Order).Select(v => v.Value)]
        })],
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc
    };
}
