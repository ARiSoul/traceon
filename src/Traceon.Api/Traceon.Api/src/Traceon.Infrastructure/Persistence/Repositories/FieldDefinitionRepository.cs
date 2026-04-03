using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class FieldDefinitionRepository(TraceonDbContext context) : IFieldDefinitionRepository
{
    public IQueryable<FieldDefinition> Query() => context.FieldDefinitions.AsNoTracking();

    public async Task<FieldDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.FieldDefinitions.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<FieldDefinition>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await context.FieldDefinitions
            .AsNoTracking()
            .Where(fd => fd.UserId == userId)
            .OrderBy(fd => fd.DefaultName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FieldDefinition fieldDefinition, CancellationToken cancellationToken = default)
    {
        context.FieldDefinitions.Add(fieldDefinition);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FieldDefinition fieldDefinition, CancellationToken cancellationToken = default)
    {
        context.FieldDefinitions.Update(fieldDefinition);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.FieldDefinitions
            .Where(fd => fd.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(fd => fd.IsDeleted, true)
                .SetProperty(fd => fd.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    public async Task<FieldDefinition?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.FieldDefinitions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(fd => fd.Id == id && fd.IsDeleted, cancellationToken);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.FieldDefinitions
            .IgnoreQueryFilters()
            .Where(fd => fd.Id == id && fd.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(fd => fd.IsDeleted, false)
                .SetProperty(fd => fd.DeletedAtUtc, (DateTime?)null), cancellationToken);
    }
}
