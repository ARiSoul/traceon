using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IActionChartVisibilityRepository
{
    Task<ActionChartVisibility?> GetByActionAndUserAsync(Guid trackedActionId, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActionChartVisibility>> GetByActionAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(ActionChartVisibility entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActionChartVisibility entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<ActionChartVisibility> entities, CancellationToken cancellationToken = default);
}
