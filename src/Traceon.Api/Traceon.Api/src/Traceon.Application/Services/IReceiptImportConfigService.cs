using Traceon.Application.Common;
using Traceon.Contracts.ReceiptImport;

namespace Traceon.Application.Services;

public interface IReceiptImportConfigService
{
    Task<Result<ReceiptImportConfigResponse>> GetByTrackedActionIdAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ReceiptImportConfigResponse>> UpsertAsync(Guid trackedActionId, UpdateReceiptImportConfigRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid trackedActionId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ReceiptMappingRuleResponse>>> GetMappingRulesAsync(Guid trackedActionId, CancellationToken cancellationToken = default);
    Task<Result<ReceiptMappingRuleResponse>> CreateMappingRuleAsync(Guid trackedActionId, CreateReceiptMappingRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<ReceiptMappingRuleResponse>> UpdateMappingRuleAsync(Guid ruleId, UpdateReceiptMappingRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteMappingRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);
}
