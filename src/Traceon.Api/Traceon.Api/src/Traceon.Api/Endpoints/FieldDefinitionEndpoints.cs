using Traceon.Api.Filters;
using Traceon.Application.Contracts.FieldDefinitions;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class FieldDefinitionEndpoints
{
    public static RouteGroupBuilder MapFieldDefinitionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/field-definitions")
            .WithTags("Field Definitions");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateFieldDefinitionRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateFieldDefinitionRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static async Task<IResult> GetAllAsync(
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAllAsync(cancellationToken);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> CreateAsync(
        CreateFieldDefinitionRequest request,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/field-definitions/{result.Value.Id}", result.Value)
            : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateFieldDefinitionRequest request,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : TypedResults.NotFound(result.Error);
    }
}
