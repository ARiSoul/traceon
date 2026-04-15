using Traceon.Api.Extensions;
using Traceon.Application.Services;
using Traceon.Contracts.DropdownValues;

namespace Traceon.Api.Endpoints;

internal static class DropdownValueEndpoints
{
    public static RouteGroupBuilder MapDropdownValueEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/dropdown-values")
            .WithTags("Dropdown Values");

        group.MapGet("/by-field/{fieldDefinitionId:guid}", GetByFieldDefinitionIdAsync);
        group.MapPut("/{id:guid}/rename", RenameAsync);
        group.MapPut("/by-field/{fieldDefinitionId:guid}/reorder", ReorderAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPost("/by-field/{fieldDefinitionId:guid}/sync", SyncAsync);

        // Metadata schema (per FieldDefinition)
        group.MapGet("/metadata-fields/all", GetAllMetadataFieldsAsync);
        group.MapGet("/metadata-fields/by-field/{fieldDefinitionId:guid}", GetMetadataFieldsAsync);
        group.MapPost("/metadata-fields", CreateMetadataFieldAsync);
        group.MapPut("/metadata-fields/{id:guid}", UpdateMetadataFieldAsync);
        group.MapDelete("/metadata-fields/{id:guid}", DeleteMetadataFieldAsync);
        group.MapPut("/metadata-fields/by-field/{fieldDefinitionId:guid}/reorder", ReorderMetadataFieldsAsync);

        // Metadata values (per DropdownValue)
        group.MapGet("/{dropdownValueId:guid}/metadata", GetMetadataValuesAsync);
        group.MapPut("/{dropdownValueId:guid}/metadata", UpsertMetadataValuesAsync);

        return group;
    }

    private static async Task<IResult> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.GetByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RenameAsync(
        Guid id,
        RenameDropdownValueRequest request,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.RenameAsync(id, request.NewValue, cancellationToken)).ToHttpResult();

    private static async Task<IResult> ReorderAsync(
        Guid fieldDefinitionId,
        ReorderDropdownValuesRequest request,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.ReorderAsync(fieldDefinitionId, request.OrderedIds, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> SyncAsync(
        Guid fieldDefinitionId,
        IDropdownValueService service,
        CancellationToken cancellationToken)
        => (await service.SyncFromFieldDefinitionAsync(fieldDefinitionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetMetadataFieldsAsync(
        Guid fieldDefinitionId,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.GetFieldsByFieldDefinitionIdAsync(fieldDefinitionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetAllMetadataFieldsAsync(
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.GetAllFieldsAsync(cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateMetadataFieldAsync(
        CreateDropdownValueMetadataFieldRequest request,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.CreateFieldAsync(request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateMetadataFieldAsync(
        Guid id,
        UpdateDropdownValueMetadataFieldRequest request,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.UpdateFieldAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteMetadataFieldAsync(
        Guid id,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.DeleteFieldAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> ReorderMetadataFieldsAsync(
        Guid fieldDefinitionId,
        ReorderDropdownValueMetadataFieldsRequest request,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.ReorderFieldsAsync(fieldDefinitionId, request.OrderedIds, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetMetadataValuesAsync(
        Guid dropdownValueId,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.GetValuesAsync(dropdownValueId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpsertMetadataValuesAsync(
        Guid dropdownValueId,
        UpsertDropdownValueMetadataRequest request,
        IDropdownValueMetadataService service,
        CancellationToken cancellationToken)
        => (await service.UpsertValuesAsync(dropdownValueId, request.Values, cancellationToken)).ToHttpResult();
}