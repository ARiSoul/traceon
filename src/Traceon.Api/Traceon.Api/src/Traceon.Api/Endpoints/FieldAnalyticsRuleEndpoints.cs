using Traceon.Api.Extensions;
using Traceon.Contracts.FieldAnalyticsRules;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class FieldAnalyticsRuleEndpoints
{
    public static RouteGroupBuilder MapFieldAnalyticsRuleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/analytics-rules")
            .WithTags("Field Analytics Rules");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        IFieldAnalyticsRuleService service,
        CancellationToken cancellationToken)
        => (await service.GetByTrackedActionIdAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateFieldAnalyticsRuleRequest request,
        IFieldAnalyticsRuleService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/analytics-rules/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateFieldAnalyticsRuleRequest request,
        IFieldAnalyticsRuleService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IFieldAnalyticsRuleService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();
}
