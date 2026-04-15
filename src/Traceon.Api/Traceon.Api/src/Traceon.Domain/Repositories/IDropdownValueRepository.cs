using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IDropdownValueRepository
{
    Task<DropdownValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdsAsync(IEnumerable<Guid> fieldDefinitionIds, CancellationToken cancellationToken = default);
    Task AddAsync(DropdownValue entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<DropdownValue> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(DropdownValue entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cascades a value rename across all related tables in a single transaction.
    /// </summary>
    Task CascadeRenameAsync(
        Guid fieldDefinitionId,
        string oldValue,
        string newValue,
        CancellationToken cancellationToken = default);
}