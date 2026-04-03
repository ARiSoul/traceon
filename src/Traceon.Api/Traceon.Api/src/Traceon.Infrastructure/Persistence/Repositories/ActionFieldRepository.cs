using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class ActionFieldRepository(TraceonDbContext context) : IActionFieldRepository
{
    public IQueryable<ActionField> Query() => context.ActionFields.AsNoTracking();

    public async Task<ActionField?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionFields.FindAsync([id], cancellationToken);
    }

    public async Task<bool> ExistsByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        return await context.ActionFields
            .AsNoTracking()
            .AnyAsync(af => af.FieldDefinitionId == fieldDefinitionId, cancellationToken);
    }

    public async Task<IReadOnlyList<ActionField>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default)
    {
        return await context.ActionFields
            .AsNoTracking()
            .Where(af => af.TrackedActionId == trackedActionId)
            .OrderBy(af => af.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ActionField actionField, CancellationToken cancellationToken = default)
    {
        context.ActionFields.Add(actionField);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ActionField actionField, CancellationToken cancellationToken = default)
    {
        context.ActionFields.Update(actionField);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ActionFields
            .Where(af => af.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(af => af.IsDeleted, true)
                .SetProperty(af => af.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<ActionField?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ActionFields
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(af => af.Id == id && af.IsDeleted, cancellationToken);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.ActionFields
            .IgnoreQueryFilters()
            .Where(af => af.Id == id && af.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(af => af.IsDeleted, false)
                .SetProperty(af => af.DeletedAtUtc, (DateTime?)null), cancellationToken);
    }
}
