using Traceon.Application.Common;
using Traceon.Contracts.FieldDependencyRules;

namespace Traceon.Application.Services;

public interface IFieldDependencyRuleService
{
    Task<Result<IReadOnlyList<FieldDependencyRuleResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<FieldDependencyRuleResponse>> CreateAsync(Guid trackedActionId, CreateFieldDependencyRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<FieldDependencyRuleResponse>> UpdateAsync(Guid id, UpdateFieldDependencyRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
