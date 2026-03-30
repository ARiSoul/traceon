using Microsoft.OData.Edm;
using Traceon.Api.Extensions;
using Traceon.Api.Filters;
using Traceon.Contracts.Tags;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class TagEndpoints
{
    public static RouteGroupBuilder MapTagEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tags")
            .WithTags("Tags");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateTagRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateTagRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static IResult GetAllAsync(
        HttpRequest request,
        ITagService service,
        IEdmModel edmModel)
    {
        var queryable = service.QueryAll()
            .ApplyODataQuery(request, edmModel);

        return TypedResults.Ok(queryable);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITagService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        CreateTagRequest request,
        ITagService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tags/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTagRequest request,
        ITagService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ITagService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();
}
