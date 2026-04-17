using Traceon.Api.Extensions;
using Traceon.Application.Services;
using Traceon.Contracts.Analytics;

namespace Traceon.Api.Endpoints;

internal static class ChartVisibilityEndpoints
{
    public static RouteGroupBuilder MapChartVisibilityEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/chart-visibility")
            .WithTags("Chart Visibility");

        group.MapGet("/", GetAsync);
        group.MapPut("/", UpsertAsync);
        group.MapPut("/order", UpsertOrderAsync);

        return group;
    }

    private static async Task<IResult> GetAsync(
        Guid trackedActionId,
        IActionChartVisibilityService service,
        CancellationToken cancellationToken)
        => (await service.GetAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpsertAsync(
        Guid trackedActionId,
        UpdateChartVisibilityRequest request,
        IActionChartVisibilityService service,
        CancellationToken cancellationToken)
        => (await service.UpsertAsync(trackedActionId, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpsertOrderAsync(
        Guid trackedActionId,
        UpdateChartOrderRequest request,
        IActionChartVisibilityService service,
        CancellationToken cancellationToken)
        => (await service.UpsertOrderAsync(trackedActionId, request, cancellationToken)).ToHttpResult();
}
