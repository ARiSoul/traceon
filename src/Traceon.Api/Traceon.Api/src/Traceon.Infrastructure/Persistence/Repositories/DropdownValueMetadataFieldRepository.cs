using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class DropdownValueMetadataFieldRepository(TraceonDbContext context) : IDropdownValueMetadataFieldRepository
{
    public async Task<DropdownValueMetadataField?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.DropdownValueMetadataFields.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<DropdownValueMetadataField>> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        return await context.DropdownValueMetadataFields
            .AsNoTracking()
            .Where(f => f.FieldDefinitionId == fieldDefinitionId)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DropdownValueMetadataField>> GetByFieldDefinitionIdsAsync(
        IEnumerable<Guid> fieldDefinitionIds, CancellationToken cancellationToken = default)
    {
        var ids = fieldDefinitionIds.ToList();
        return await context.DropdownValueMetadataFields
            .AsNoTracking()
            .Where(f => ids.Contains(f.FieldDefinitionId))
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DropdownValueMetadataField entity, CancellationToken cancellationToken = default)
    {
        context.DropdownValueMetadataFields.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DropdownValueMetadataField entity, CancellationToken cancellationToken = default)
    {
        context.DropdownValueMetadataFields.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.DropdownValueMetadataFields.FindAsync([id], cancellationToken);
        if (entity is null) return;
        entity.MarkDeleted();
        await context.SaveChangesAsync(cancellationToken);
    }
}
