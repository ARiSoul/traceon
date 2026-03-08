using Traceon.Api.Filters;
using Traceon.Contracts.TrackedActions;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class TrackedActionEndpoints
{
    public static RouteGroupBuilder MapTrackedActionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions")
            .WithTags("Tracked Actions");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateTrackedActionRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateTrackedActionRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPost("/{id:guid}/tags/{tagId:guid}", AddTagAsync);
        group.MapDelete("/{id:guid}/tags/{tagId:guid}", RemoveTagAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> CreateAsync(
        CreateTrackedActionRequest request,
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/tracked-actions/{result.Value.Id}", result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTrackedActionRequest request,
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> AddTagAsync(
        Guid id,
        Guid tagId,
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.AddTagAsync(id, tagId, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> RemoveTagAsync(
        Guid id,
        Guid tagId,
        ITrackedActionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RemoveTagAsync(id, tagId, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
