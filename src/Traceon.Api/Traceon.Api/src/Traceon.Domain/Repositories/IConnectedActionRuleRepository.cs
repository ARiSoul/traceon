using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IConnectedActionRuleRepository
{
    Task<ConnectedActionRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConnectedActionRule>> GetBySourceTrackedActionIdAsync(Guid sourceTrackedActionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConnectedActionRule>> GetByTargetTrackedActionIdAsync(Guid targetTrackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(ConnectedActionRule entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConnectedActionRule entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
