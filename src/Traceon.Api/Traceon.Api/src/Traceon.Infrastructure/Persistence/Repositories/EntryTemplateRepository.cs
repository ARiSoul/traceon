using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class EntryTemplateRepository(TraceonDbContext context) : IEntryTemplateRepository
{
    public async Task<EntryTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.EntryTemplates.FindAsync([id], cancellationToken);
    }

    public async Task<EntryTemplate?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.EntryTemplates
            .Include(t => t.Fields)
                .ThenInclude(f => f.Values)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EntryTemplate>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.EntryTemplates
            .AsNoTracking()
            .Include(t => t.Fields)
                .ThenInclude(f => f.Values)
            .Where(t => t.TrackedActionId == trackedActionId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EntryTemplate template, CancellationToken cancellationToken = default)
    {
        context.EntryTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EntryTemplate template, IReadOnlyList<(Guid ActionFieldId, IReadOnlyList<string> Values)>? fieldValues = null, CancellationToken cancellationToken = default)
    {
        if (fieldValues is not null)
        {
            var existingFields = await context.EntryTemplateFields
                .Include(f => f.Values)
                .Where(f => f.EntryTemplateId == template.Id)
                .ToListAsync(cancellationToken);

            var incomingByFieldId = fieldValues.ToDictionary(f => f.ActionFieldId, f => f.Values);

            foreach (var existing in existingFields.Where(f => !incomingByFieldId.ContainsKey(f.ActionFieldId)).ToList())
                context.EntryTemplateFields.Remove(existing);

            foreach (var (actionFieldId, values) in fieldValues)
            {
                var existing = existingFields.FirstOrDefault(f => f.ActionFieldId == actionFieldId);
                if (existing is not null)
                {
                    existing.SetValues(values);
                }
                else
                {
                    var slot = EntryTemplateField.Create(template.Id, actionFieldId);
                    slot.SetValues(values);
                    context.EntryTemplateFields.Add(slot);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.EntryTemplates
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsDeleted, true)
                .SetProperty(t => t.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<EntryTemplate?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.EntryTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted, cancellationToken);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.EntryTemplates
            .IgnoreQueryFilters()
            .Where(t => t.Id == id && t.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsDeleted, false)
                .SetProperty(t => t.DeletedAtUtc, (DateTime?)null), cancellationToken);
    }

    public async Task<bool> IsNameTakenAsync(Guid trackedActionId, string name, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var lowered = name.Trim().ToLower();
        return await context.EntryTemplates
            .AsNoTracking()
            .Where(t => t.TrackedActionId == trackedActionId
                        && t.Name.ToLower() == lowered
                        && (!excludeId.HasValue || t.Id != excludeId.Value))
            .AnyAsync(cancellationToken);
    }
}
