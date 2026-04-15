using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IDropdownValueMetadataFieldRepository
{
    Task<DropdownValueMetadataField?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValueMetadataField>> GetByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValueMetadataField>> GetByFieldDefinitionIdsAsync(IEnumerable<Guid> fieldDefinitionIds, CancellationToken cancellationToken = default);
    Task AddAsync(DropdownValueMetadataField entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(DropdownValueMetadataField entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
