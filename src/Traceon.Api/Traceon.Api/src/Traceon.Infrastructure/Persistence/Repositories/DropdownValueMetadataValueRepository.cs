using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class DropdownValueMetadataValueRepository(TraceonDbContext context) : IDropdownValueMetadataValueRepository
{
    public async Task<IReadOnlyList<DropdownValueMetadataValue>> GetByDropdownValueIdAsync(
        Guid dropdownValueId, CancellationToken cancellationToken = default)
    {
        return await context.DropdownValueMetadataValues
            .AsNoTracking()
            .Where(v => v.DropdownValueId == dropdownValueId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DropdownValueMetadataValue>> GetByDropdownValueIdsAsync(
        IEnumerable<Guid> dropdownValueIds, CancellationToken cancellationToken = default)
    {
        var ids = dropdownValueIds.ToList();
        return await context.DropdownValueMetadataValues
            .AsNoTracking()
            .Where(v => ids.Contains(v.DropdownValueId))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(
        Guid dropdownValueId,
        IReadOnlyDictionary<Guid, string?> valuesByMetadataFieldId,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.DropdownValueMetadataValues
            .Where(v => v.DropdownValueId == dropdownValueId)
            .ToListAsync(cancellationToken);

        var existingByField = existing.ToDictionary(v => v.MetadataFieldId);

        foreach (var (metadataFieldId, value) in valuesByMetadataFieldId)
        {
            if (existingByField.TryGetValue(metadataFieldId, out var row))
            {
                row.UpdateValue(value);
            }
            else
            {
                var entity = DropdownValueMetadataValue.Create(dropdownValueId, metadataFieldId, value);
                context.DropdownValueMetadataValues.Add(entity);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByMetadataFieldIdAsync(Guid metadataFieldId, CancellationToken cancellationToken = default)
    {
        await context.DropdownValueMetadataValues
            .Where(v => v.MetadataFieldId == metadataFieldId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByDropdownValueIdAsync(Guid dropdownValueId, CancellationToken cancellationToken = default)
    {
        await context.DropdownValueMetadataValues
            .Where(v => v.DropdownValueId == dropdownValueId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
