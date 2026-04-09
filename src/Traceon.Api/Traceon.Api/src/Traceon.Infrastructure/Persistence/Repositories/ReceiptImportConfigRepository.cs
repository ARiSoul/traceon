using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ReceiptImportConfigRepository(TraceonDbContext context) : IReceiptImportConfigRepository
{
    public IQueryable<ReceiptImportConfig> Query() => context.ReceiptImportConfigs.AsNoTracking();

    public async Task<HashSet<Guid>> GetActionIdsWithConfigAsync(IEnumerable<Guid> actionIds, CancellationToken cancellationToken = default)
    {
        var ids = actionIds.ToList();
        var result = await context.ReceiptImportConfigs
            .AsNoTracking()
            .Where(c => ids.Contains(c.TrackedActionId))
            .Select(c => c.TrackedActionId)
            .ToListAsync(cancellationToken);
        return [.. result];
    }

    public async Task<ReceiptImportConfig?> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ReceiptImportConfigs
            .Include(c => c.MappingRules)
            .FirstOrDefaultAsync(c => c.TrackedActionId == trackedActionId, cancellationToken);
    }

    public async Task AddAsync(ReceiptImportConfig entity, CancellationToken cancellationToken = default)
    {
        context.ReceiptImportConfigs.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ReceiptImportConfig entity, CancellationToken cancellationToken = default)
    {
        context.ReceiptImportConfigs.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ReceiptImportConfigs
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsDeleted, true)
                .SetProperty(c => c.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<ReceiptMappingRule?> GetMappingRuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ReceiptMappingRules.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<ReceiptMappingRule>> GetMappingRulesByConfigIdAsync(Guid configId, CancellationToken cancellationToken = default)
    {
        return await context.ReceiptMappingRules
            .AsNoTracking()
            .Where(r => r.ReceiptImportConfigId == configId)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task AddMappingRuleAsync(ReceiptMappingRule entity, CancellationToken cancellationToken = default)
    {
        context.ReceiptMappingRules.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMappingRuleAsync(ReceiptMappingRule entity, CancellationToken cancellationToken = default)
    {
        context.ReceiptMappingRules.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMappingRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ReceiptMappingRules
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsDeleted, true)
                .SetProperty(r => r.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }
}
