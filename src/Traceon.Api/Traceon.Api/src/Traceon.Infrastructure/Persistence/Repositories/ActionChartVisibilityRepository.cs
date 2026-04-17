using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ActionChartVisibilityRepository(TraceonDbContext context) : IActionChartVisibilityRepository
{
    public async Task<ActionChartVisibility?> GetByActionAndUserAsync(Guid trackedActionId, string userId, CancellationToken cancellationToken = default)
    {
        return await context.ActionChartVisibilities
            .FirstOrDefaultAsync(e => e.TrackedActionId == trackedActionId && e.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ActionChartVisibility>> GetByActionAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ActionChartVisibilities
            .Where(e => e.TrackedActionId == trackedActionId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ActionChartVisibility entity, CancellationToken cancellationToken = default)
    {
        context.ActionChartVisibilities.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ActionChartVisibility entity, CancellationToken cancellationToken = default)
    {
        context.ActionChartVisibilities.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<ActionChartVisibility> entities, CancellationToken cancellationToken = default)
    {
        context.ActionChartVisibilities.UpdateRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }
}
