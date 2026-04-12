using Traceon.Application.Common;
using Traceon.Contracts.ConnectedActionRules;

namespace Traceon.Application.Services;

public interface IConnectedActionRuleService
{
    Task<Result<IReadOnlyList<ConnectedActionRuleResponse>>> GetBySourceTrackedActionIdAsync(Guid sourceTrackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ConnectedActionRuleResponse>> CreateAsync(Guid sourceTrackedActionId, CreateConnectedActionRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<ConnectedActionRuleResponse>> UpdateAsync(Guid id, UpdateConnectedActionRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
