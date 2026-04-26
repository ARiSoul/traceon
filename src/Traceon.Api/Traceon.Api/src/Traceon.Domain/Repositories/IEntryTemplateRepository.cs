using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IEntryTemplateRepository
{
    Task<EntryTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EntryTemplate?> GetByIdWithFieldsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EntryTemplate>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task AddAsync(EntryTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(EntryTemplate template, IReadOnlyList<(Guid ActionFieldId, IReadOnlyList<string> Values)>? fieldValues = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EntryTemplate?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task RestoreAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsNameTakenAsync(Guid trackedActionId, string name, Guid? excludeId, CancellationToken cancellationToken = default);
}
