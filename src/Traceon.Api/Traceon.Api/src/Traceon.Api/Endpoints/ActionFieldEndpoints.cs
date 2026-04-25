using Traceon.Api.Extensions;
using Traceon.Api.Filters;
using Traceon.Contracts.ActionFields;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class ActionFieldEndpoints
{
    public static RouteGroupBuilder MapActionFieldEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/fields")
            .WithTags("Action Fields");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateActionFieldRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateActionFieldRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPost("/{id:guid}/restore", RestoreAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        IActionFieldService service,
        CancellationToken cancellationToken)
        // Materialized path so AutoCounterConfig (deserialized from JSON) round-trips.
        // OData was unused on this endpoint and the JSON column can't translate to SQL anyway.
        => (await service.GetByTrackedActionIdAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetByIdAsync(
        Guid trackedActionId,
        Guid id,
        IActionFieldService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateActionFieldRequest request,
        IActionFieldService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/fields/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateActionFieldRequest request,
        IActionFieldService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IActionFieldService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RestoreAsync(
        Guid trackedActionId,
        Guid id,
        IActionFieldService service,
        CancellationToken cancellationToken)
        => (await service.RestoreAsync(id, cancellationToken)).ToHttpResult();
}
