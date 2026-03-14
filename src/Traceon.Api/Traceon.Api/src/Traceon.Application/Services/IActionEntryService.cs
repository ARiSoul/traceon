using Traceon.Application.Common;
using Traceon.Contracts.ActionEntries;

namespace Traceon.Application.Services;

public interface IActionEntryService
{
    Task<Result<IQueryable<ActionEntryResponse>>> QueryByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ActionEntryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ActionEntryResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ActionEntryResponse>> CreateAsync(Guid trackedActionId, CreateActionEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<ActionEntryResponse>> UpdateAsync(Guid id, UpdateActionEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
