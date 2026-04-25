using Microsoft.OData.Edm;
using Traceon.Api.Extensions;
using Traceon.Api.Filters;
using Traceon.Contracts.ActionEntries;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class ActionEntryEndpoints
{
    public static RouteGroupBuilder MapActionEntryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/entries")
            .WithTags("Action Entries");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateActionEntryRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateActionEntryRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPost("/{id:guid}/restore", RestoreAsync);
        group.MapPost("/bulk-delete", BulkDeleteAsync).AddEndpointFilter<ValidationFilter<BulkDeleteEntriesRequest>>();
        group.MapPost("/bulk-update-fields", BulkUpdateFieldsAsync).AddEndpointFilter<ValidationFilter<BulkUpdateEntryFieldsRequest>>();
        group.MapPost("/auto-counter-preview", PreviewAutoCounterAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        HttpRequest request,
        IActionEntryService service,
        IEdmModel edmModel,
        CancellationToken cancellationToken)
    {
        var result = await service.QueryByTrackedActionIdAsync(trackedActionId, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.NotFound(result.Error);

        return TypedResults.Ok(result.Value.ApplyODataQuery(request, edmModel));
    }

    private static async Task<IResult> GetByIdAsync(
        Guid trackedActionId,
        Guid id,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateActionEntryRequest request,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/entries/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateActionEntryRequest request,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RestoreAsync(
        Guid trackedActionId,
        Guid id,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.RestoreAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> BulkDeleteAsync(
        Guid trackedActionId,
        BulkDeleteEntriesRequest request,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.BulkDeleteAsync(trackedActionId, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> BulkUpdateFieldsAsync(
        Guid trackedActionId,
        BulkUpdateEntryFieldsRequest request,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.BulkUpdateFieldsAsync(trackedActionId, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> PreviewAutoCounterAsync(
        Guid trackedActionId,
        AutoCounterPreviewRequest request,
        IActionEntryService service,
        CancellationToken cancellationToken)
        => (await service.PreviewAutoCounterAsync(trackedActionId, request, cancellationToken)).ToHttpResult();
}
