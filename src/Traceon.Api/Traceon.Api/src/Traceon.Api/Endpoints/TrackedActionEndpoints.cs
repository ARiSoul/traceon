using Microsoft.OData.Edm;
using Traceon.Api.Extensions;
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
        group.MapPost("/{id:guid}/restore", RestoreAsync);
        group.MapGet("/{id:guid}/tags", GetTagsAsync);
        group.MapPost("/{id:guid}/tags/{tagId:guid}", AddTagAsync);
        group.MapDelete("/{id:guid}/tags/{tagId:guid}", RemoveTagAsync);
        group.MapPut("/reorder", ReorderAsync);

        return group;
    }

    private static IResult GetAllAsync(
        HttpRequest request,
        ITrackedActionService service,
        IEdmModel edmModel)
    {
        var queryable = service.QueryAll()
            .ApplyODataQuery(request, edmModel);

        return TypedResults.Ok(queryable);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        CreateTrackedActionRequest request,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTrackedActionRequest request,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RestoreAsync(
        Guid id,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.RestoreAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetTagsAsync(
        Guid id,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.GetTagsAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> AddTagAsync(
        Guid id,
        Guid tagId,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.AddTagAsync(id, tagId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RemoveTagAsync(
        Guid id,
        Guid tagId,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.RemoveTagAsync(id, tagId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> ReorderAsync(
        List<Guid> orderedIds,
        ITrackedActionService service,
        CancellationToken cancellationToken)
        => (await service.ReorderAsync(orderedIds, cancellationToken)).ToHttpResult();
}
