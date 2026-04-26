using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ActionEntryRepository(TraceonDbContext context) : IActionEntryRepository
{
    public IQueryable<ActionEntry> Query() => context.ActionEntries.AsNoTracking();

    public async Task<ActionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<ActionEntryField>> GetActionEntryFieldsAsync(Guid id, bool asNoTracking, CancellationToken cancellationToken = default)
    {
        var query = context.ActionEntryFields
            .Include(f => f.Values)
            .Where(f => f.ActionEntryId == id);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ActionEntry?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .Include(e => e.Fields)
                .ThenInclude(f => f.Values)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ActionEntry>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .AsNoTracking()
            .Include(e => e.Fields)
                .ThenInclude(f => f.Values)
            .Where(e => e.TrackedActionId == trackedActionId)
            .OrderByDescending(e => e.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ActionEntry entry, CancellationToken cancellationToken = default)
    {
        context.ActionEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ActionEntry entry, IReadOnlyList<(Guid ActionFieldId, IReadOnlyList<string> Values)>? fieldValues = null, CancellationToken cancellationToken = default)
    {
        if (fieldValues is not null)
        {
            var existingFields = await context.ActionEntryFields
                .Include(f => f.Values)
                .Where(f => f.ActionEntryId == entry.Id)
                .ToListAsync(cancellationToken);

            var incomingByFieldId = fieldValues.ToDictionary(f => f.ActionFieldId, f => f.Values);

            foreach (var existing in existingFields.Where(f => !incomingByFieldId.ContainsKey(f.ActionFieldId)).ToList())
                context.ActionEntryFields.Remove(existing);

            foreach (var (actionFieldId, values) in fieldValues)
            {
                var existing = existingFields.FirstOrDefault(f => f.ActionFieldId == actionFieldId);
                if (existing is not null)
                {
                    existing.SetValues(values);
                }
                else
                {
                    var slot = ActionEntryField.Create(entry.Id, actionFieldId);
                    slot.SetValues(values);
                    context.ActionEntryFields.Add(slot);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ActionEntries
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<int> DeleteManyAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .Where(e => ids.Contains(e.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.IsDeleted, true)
                .SetProperty(e => e.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<IReadOnlyList<ActionEntry>> GetByIdsWithFieldsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .Include(e => e.Fields)
                .ThenInclude(f => f.Values)
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<ActionEntry?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted, cancellationToken);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ActionEntries
            .IgnoreQueryFilters()
            .Where(e => e.Id == id && e.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.IsDeleted, false)
                .SetProperty(e => e.DeletedAtUtc, (DateTime?)null), cancellationToken);
    }
}
