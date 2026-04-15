using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IDropdownValueMetadataValueRepository
{
    Task<IReadOnlyList<DropdownValueMetadataValue>> GetByDropdownValueIdAsync(Guid dropdownValueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DropdownValueMetadataValue>> GetByDropdownValueIdsAsync(IEnumerable<Guid> dropdownValueIds, CancellationToken cancellationToken = default);
    Task UpsertAsync(Guid dropdownValueId, IReadOnlyDictionary<Guid, string?> valuesByMetadataFieldId, CancellationToken cancellationToken = default);
    Task DeleteByMetadataFieldIdAsync(Guid metadataFieldId, CancellationToken cancellationToken = default);
    Task DeleteByDropdownValueIdAsync(Guid dropdownValueId, CancellationToken cancellationToken = default);
}
