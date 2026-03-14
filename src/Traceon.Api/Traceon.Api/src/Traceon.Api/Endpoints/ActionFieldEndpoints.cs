using Microsoft.OData.Edm;
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

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        HttpRequest request,
        IActionFieldService service,
        IEdmModel edmModel,
        CancellationToken cancellationToken)
    {
        var result = await service.QueryByTrackedActionIdAsync(trackedActionId, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(result.Value.ApplyODataQuery(request, edmModel));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid trackedActionId,
        Guid id,
        IActionFieldService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateActionFieldRequest request,
        IActionFieldService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(trackedActionId, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/tracked-actions/{trackedActionId}/fields/{result.Value.Id}", result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateActionFieldRequest request,
        IActionFieldService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IActionFieldService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
