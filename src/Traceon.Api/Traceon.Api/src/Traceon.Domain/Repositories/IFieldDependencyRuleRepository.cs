using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IFieldDependencyRuleRepository
{
    Task<FieldDependencyRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FieldDependencyRule>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(FieldDependencyRule entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FieldDependencyRule entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
