using Traceon.Api.Extensions;
using Traceon.Application.Services;
using Traceon.Contracts.ReceiptImport;

namespace Traceon.Api.Endpoints;

internal static class ReceiptImportEndpoints
{
    public static RouteGroupBuilder MapReceiptImportEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/receipt-config")
            .WithTags("Receipt Import Config");

        group.MapGet("/", GetConfigAsync);
        group.MapPut("/", UpsertConfigAsync);
        group.MapDelete("/", DeleteConfigAsync);

        group.MapGet("/mapping-rules", GetMappingRulesAsync);
        group.MapPost("/mapping-rules", CreateMappingRuleAsync);
        group.MapPut("/mapping-rules/{ruleId:guid}", UpdateMappingRuleAsync);
        group.MapDelete("/mapping-rules/{ruleId:guid}", DeleteMappingRuleAsync);

        return group;
    }

    private static async Task<IResult> GetConfigAsync(
        Guid trackedActionId,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.GetByTrackedActionIdAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpsertConfigAsync(
        Guid trackedActionId,
        UpdateReceiptImportConfigRequest request,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.UpsertAsync(trackedActionId, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteConfigAsync(
        Guid trackedActionId,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetMappingRulesAsync(
        Guid trackedActionId,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.GetMappingRulesAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateMappingRuleAsync(
        Guid trackedActionId,
        CreateReceiptMappingRuleRequest request,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.CreateMappingRuleAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/receipt-config/mapping-rules/{v.Id}");

    private static async Task<IResult> UpdateMappingRuleAsync(
        Guid trackedActionId,
        Guid ruleId,
        UpdateReceiptMappingRuleRequest request,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.UpdateMappingRuleAsync(ruleId, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteMappingRuleAsync(
        Guid trackedActionId,
        Guid ruleId,
        IReceiptImportConfigService service,
        CancellationToken cancellationToken)
        => (await service.DeleteMappingRuleAsync(ruleId, cancellationToken)).ToHttpResult();
}
