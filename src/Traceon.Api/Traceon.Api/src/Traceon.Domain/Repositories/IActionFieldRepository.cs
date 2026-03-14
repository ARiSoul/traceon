using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IActionFieldRepository
{
    IQueryable<ActionField> Query();
    Task<ActionField?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActionField>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(ActionField actionField, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActionField actionField, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
