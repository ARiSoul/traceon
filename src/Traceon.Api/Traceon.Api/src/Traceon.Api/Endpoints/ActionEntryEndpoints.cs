using Traceon.Api.Filters;
using Traceon.Application.Contracts.ActionEntries;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class ActionEntryEndpoints
{
    public static RouteGroupBuilder MapActionEntryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/entries")
            .WithTags("Action Entries");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateActionEntryRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateActionEntryRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        IActionEntryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByTrackedActionIdAsync(trackedActionId, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid trackedActionId,
        Guid id,
        IActionEntryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateActionEntryRequest request,
        IActionEntryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(trackedActionId, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/tracked-actions/{trackedActionId}/entries/{result.Value.Id}", result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateActionEntryRequest request,
        IActionEntryService service,
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
        IActionEntryService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
