using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IReceiptImportConfigRepository
{
    IQueryable<ReceiptImportConfig> Query();
    Task<ReceiptImportConfig?> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> GetActionIdsWithConfigAsync(IEnumerable<Guid> actionIds, CancellationToken cancellationToken = default);
    Task AddAsync(ReceiptImportConfig entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReceiptImportConfig entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ReceiptMappingRule?> GetMappingRuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReceiptMappingRule>> GetMappingRulesByConfigIdAsync(Guid configId, CancellationToken cancellationToken = default);
    Task AddMappingRuleAsync(ReceiptMappingRule entity, CancellationToken cancellationToken = default);
    Task UpdateMappingRuleAsync(ReceiptMappingRule entity, CancellationToken cancellationToken = default);
    Task DeleteMappingRuleAsync(Guid id, CancellationToken cancellationToken = default);
}
