using Traceon.Infrastructure;

namespace Traceon.Api.Endpoints;

internal static class TrashEndpoints
{
    public static IEndpointRouteBuilder MapTrashEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/trash", GetDeletedItemsAsync)
            .WithTags("Trash")
            .RequireAuthorization();

        return routes;
    }

    private static async Task<IResult> GetDeletedItemsAsync(
        TrashService service,
        CancellationToken cancellationToken)
    {
        var items = await service.GetDeletedItemsAsync(cancellationToken);
        return TypedResults.Ok(items);
    }
}
