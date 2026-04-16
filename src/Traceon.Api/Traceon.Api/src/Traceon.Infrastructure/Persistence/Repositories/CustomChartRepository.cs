using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class CustomChartRepository(TraceonDbContext context) : ICustomChartRepository
{
    public async Task<CustomChart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.CustomCharts.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<CustomChart>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.CustomCharts
            .AsNoTracking()
            .Where(c => c.TrackedActionId == trackedActionId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CustomChart entity, CancellationToken cancellationToken = default)
    {
        context.CustomCharts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CustomChart entity, CancellationToken cancellationToken = default)
    {
        context.CustomCharts.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.CustomCharts
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.IsDeleted, true)
                .SetProperty(c => c.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }
}
