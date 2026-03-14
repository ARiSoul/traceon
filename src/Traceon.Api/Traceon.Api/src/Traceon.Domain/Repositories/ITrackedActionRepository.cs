using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface ITrackedActionRepository
{
    IQueryable<TrackedAction> Query();
    Task<TrackedAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TrackedAction?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TrackedAction?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrackedAction>> GetAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string userId, string name, CancellationToken cancellationToken = default);
    Task AddAsync(TrackedAction trackedAction, CancellationToken cancellationToken = default);
    Task UpdateAsync(TrackedAction trackedAction, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
