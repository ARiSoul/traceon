using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class TrackedActionRepository(TraceonDbContext context) : ITrackedActionRepository
{
    public IQueryable<TrackedAction> Query() => context.TrackedActions.AsNoTracking();

    public async Task<TrackedAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TrackedActions.FindAsync([id], cancellationToken);
    }

    public async Task<TrackedAction?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TrackedActions
            .Include(a => a.Fields)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<TrackedAction?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TrackedActions
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrackedAction>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await context.TrackedActions
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string userId, string name, CancellationToken cancellationToken = default)
    {
        return await context.TrackedActions
            .AnyAsync(a => a.UserId == userId && a.Name == name, cancellationToken);
    }

    public async Task AddAsync(TrackedAction trackedAction, CancellationToken cancellationToken = default)
    {
        context.TrackedActions.Add(trackedAction);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TrackedAction trackedAction, CancellationToken cancellationToken = default)
    {
        context.TrackedActions.Update(trackedAction);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.TrackedActions
            .Where(a => a.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
