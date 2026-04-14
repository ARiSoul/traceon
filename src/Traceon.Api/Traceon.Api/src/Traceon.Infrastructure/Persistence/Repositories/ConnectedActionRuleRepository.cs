using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ConnectedActionRuleRepository(TraceonDbContext context) : IConnectedActionRuleRepository
{
    public async Task<ConnectedActionRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ConnectedActionRules.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectedActionRule>> GetBySourceTrackedActionIdAsync(Guid sourceTrackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ConnectedActionRules
            .AsNoTracking()
            .Where(r => r.SourceTrackedActionId == sourceTrackedActionId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectedActionRule>> GetByTargetTrackedActionIdAsync(Guid targetTrackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ConnectedActionRules
            .AsNoTracking()
            .Where(r => r.TargetTrackedActionId == targetTrackedActionId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ConnectedActionRule entity, CancellationToken cancellationToken = default)
    {
        context.ConnectedActionRules.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ConnectedActionRule entity, CancellationToken cancellationToken = default)
    {
        context.ConnectedActionRules.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ConnectedActionRules
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsDeleted, true)
                .SetProperty(r => r.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }
}
