using Traceon.Application.Common;
using Traceon.Contracts.ActionFields;

namespace Traceon.Application.Services;

public interface IActionFieldService
{
    Task<Result<ActionFieldResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ActionFieldResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ActionFieldResponse>> CreateAsync(Guid trackedActionId, CreateActionFieldRequest request, CancellationToken cancellationToken = default);
    Task<Result<ActionFieldResponse>> UpdateAsync(Guid id, UpdateActionFieldRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
