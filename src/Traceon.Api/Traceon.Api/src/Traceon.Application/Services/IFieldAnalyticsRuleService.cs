using Traceon.Application.Common;
using Traceon.Contracts.FieldAnalyticsRules;

namespace Traceon.Application.Services;

public interface IFieldAnalyticsRuleService
{
    Task<Result<IReadOnlyList<FieldAnalyticsRuleResponse>>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<FieldAnalyticsRuleResponse>> CreateAsync(Guid trackedActionId, CreateFieldAnalyticsRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<FieldAnalyticsRuleResponse>> UpdateAsync(Guid id, UpdateFieldAnalyticsRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
