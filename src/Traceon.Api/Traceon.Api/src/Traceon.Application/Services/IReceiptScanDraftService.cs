using Traceon.Application.Common;
using Traceon.Contracts.ReceiptScanDraft;

namespace Traceon.Application.Services;

public interface IReceiptScanDraftService
{
    Task<Result<IReadOnlyList<ReceiptScanDraftResponse>>> GetMyDraftsAsync(CancellationToken cancellationToken = default);
    Task<Result<ReceiptScanDraftResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ReceiptScanDraftResponse>> CreateAsync(CreateReceiptScanDraftRequest request, CancellationToken cancellationToken = default);
    Task<Result<ReceiptScanDraftResponse>> UpdateAsync(Guid id, UpdateReceiptScanDraftRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
