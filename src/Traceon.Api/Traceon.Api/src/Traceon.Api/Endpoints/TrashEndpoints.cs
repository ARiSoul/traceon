using Traceon.Api.Extensions;
using Traceon.Infrastructure;

namespace Traceon.Api.Endpoints;

internal static class TrashEndpoints
{
    public static IEndpointRouteBuilder MapTrashEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/trash")
            .WithTags("Trash")
            .RequireAuthorization();

        group.MapGet("/", GetDeletedItemsAsync);
        group.MapGet("/{type}/{id:guid}/preview", GetPreviewAsync);
        group.MapDelete("/", ClearAllAsync);
        group.MapPost("/permanent-delete", PermanentlyDeleteManyAsync);

        return routes;
    }

    private static async Task<IResult> GetDeletedItemsAsync(
        TrashService service,
        CancellationToken cancellationToken)
    {
        var items = await service.GetDeletedItemsAsync(cancellationToken);
        return TypedResults.Ok(items);
    }

    private static async Task<IResult> GetPreviewAsync(
        string type,
        Guid id,
        TrashService service,
        CancellationToken cancellationToken)
        => (await service.GetPreviewAsync(type, id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> ClearAllAsync(
        TrashService service,
        CancellationToken cancellationToken)
        => (await service.ClearAllAsync(cancellationToken)).ToHttpResult();

    private static async Task<IResult> PermanentlyDeleteManyAsync(
        List<TrashItemRef> items,
        TrashService service,
        CancellationToken cancellationToken)
        => (await service.PermanentlyDeleteManyAsync(items, cancellationToken)).ToHttpResult();
}
