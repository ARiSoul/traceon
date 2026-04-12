using Traceon.Api.Extensions;
using Traceon.Contracts.ConnectedActionRules;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class ConnectedActionRuleEndpoints
{
    public static RouteGroupBuilder MapConnectedActionRuleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/connected-action-rules")
            .WithTags("Connected Action Rules");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        IConnectedActionRuleService service,
        CancellationToken cancellationToken)
        => (await service.GetBySourceTrackedActionIdAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateConnectedActionRuleRequest request,
        IConnectedActionRuleService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/connected-action-rules/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateConnectedActionRuleRequest request,
        IConnectedActionRuleService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IConnectedActionRuleService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();
}
