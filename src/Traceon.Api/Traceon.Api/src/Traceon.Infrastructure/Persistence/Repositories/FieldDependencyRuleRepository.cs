using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class FieldDependencyRuleRepository(TraceonDbContext context) : IFieldDependencyRuleRepository
{
    public async Task<FieldDependencyRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.FieldDependencyRules.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<FieldDependencyRule>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.FieldDependencyRules
            .AsNoTracking()
            .Where(r => r.TrackedActionId == trackedActionId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FieldDependencyRule entity, CancellationToken cancellationToken = default)
    {
        context.FieldDependencyRules.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FieldDependencyRule entity, CancellationToken cancellationToken = default)
    {
        context.FieldDependencyRules.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.FieldDependencyRules
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsDeleted, true)
                .SetProperty(r => r.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }
}
