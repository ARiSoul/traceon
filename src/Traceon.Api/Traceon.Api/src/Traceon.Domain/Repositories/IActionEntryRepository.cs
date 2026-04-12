using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IActionEntryRepository
{
    IQueryable<ActionEntry> Query();
    Task<ActionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionEntry?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActionEntry>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(ActionEntry entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActionEntry entry, IReadOnlyList<(Guid ActionFieldId, string? Value)>? fieldValues = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> DeleteManyAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActionEntry>> GetByIdsWithFieldsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
    Task<ActionEntry?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);
}
