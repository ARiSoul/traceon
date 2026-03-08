using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ActionEntryRepository(TraceonDbContext context) : IActionEntryRepository
{
    public async Task<ActionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries.FindAsync([id], cancellationToken);
    }

    public async Task<ActionEntry?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .Include(e => e.Fields)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ActionEntry>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ActionEntries
            .AsNoTracking()
            .Include(e => e.Fields)
            .Where(e => e.TrackedActionId == trackedActionId)
            .OrderByDescending(e => e.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ActionEntry entry, CancellationToken cancellationToken = default)
    {
        context.ActionEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ActionEntry entry, CancellationToken cancellationToken = default)
    {
        context.ActionEntries.Update(entry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ActionEntries
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
