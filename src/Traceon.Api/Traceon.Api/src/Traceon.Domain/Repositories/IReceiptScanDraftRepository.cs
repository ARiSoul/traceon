using Traceon.Domain.Entities;

namespace Traceon.Domain.Repositories;

public interface IReceiptScanDraftRepository
{
    Task<IReadOnlyList<ReceiptScanDraft>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<ReceiptScanDraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ReceiptScanDraft entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ReceiptScanDraft entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
