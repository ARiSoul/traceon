using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IFieldAnalyticsRuleRepository
{
    Task<FieldAnalyticsRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FieldAnalyticsRule>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(FieldAnalyticsRule entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FieldAnalyticsRule entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
