using Microsoft.OData.Edm;
using Traceon.Api.Extensions;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class EntryEndpoints
{
    public static RouteGroupBuilder MapEntryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/entries")
            .WithTags("Entries");

        group.MapGet("/", GetAllAsync);

        return group;
    }

    private static IResult GetAllAsync(
        HttpRequest request,
        IActionEntryService service,
        IEdmModel edmModel)
    {
        var query = service.QueryAll();
        return TypedResults.Ok(query.ApplyODataQuery(request, edmModel, maxTop: 10_000));
    }
}
