using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class FieldAnalyticsRuleRepository(TraceonDbContext context) : IFieldAnalyticsRuleRepository
{
    public async Task<FieldAnalyticsRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.FieldAnalyticsRules.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<FieldAnalyticsRule>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.FieldAnalyticsRules
            .AsNoTracking()
            .Where(r => r.TrackedActionId == trackedActionId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FieldAnalyticsRule entity, CancellationToken cancellationToken = default)
    {
        context.FieldAnalyticsRules.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FieldAnalyticsRule entity, CancellationToken cancellationToken = default)
    {
        context.FieldAnalyticsRules.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.FieldAnalyticsRules
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
}