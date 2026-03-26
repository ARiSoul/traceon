using Microsoft.OData.Edm;
using Traceon.Api.Extensions;
using Traceon.Api.Filters;
using Traceon.Contracts.FieldDefinitions;
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

    private static IResult GetAllAsync(
        HttpRequest request,
        IFieldDefinitionService service,
        IEdmModel edmModel)
    {
        var queryable = service.QueryAll()
            .ApplyODataQuery(request, edmModel);

        return TypedResults.Ok(queryable);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        CreateFieldDefinitionRequest request,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/field-definitions/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateFieldDefinitionRequest request,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IFieldDefinitionService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();
}
