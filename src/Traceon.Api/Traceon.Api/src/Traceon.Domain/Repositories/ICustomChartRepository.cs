using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface ICustomChartRepository
{
    Task<CustomChart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomChart>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(CustomChart entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomChart entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
