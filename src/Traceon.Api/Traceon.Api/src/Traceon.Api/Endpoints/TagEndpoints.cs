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

    private static async Task<IResult> GetAllAsync(
        ITagService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITagService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> CreateAsync(
        CreateTagRequest request,
        ITagService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/tags/{result.Value.Id}", result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTagRequest request,
        ITagService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ITagService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
