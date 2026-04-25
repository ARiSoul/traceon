using Traceon.Api.Extensions;
using Traceon.Api.Filters;
using Traceon.Application.Services;
using Traceon.Contracts.EntryTemplates;

namespace Traceon.Api.Endpoints;

internal static class EntryTemplateEndpoints
{
    public static RouteGroupBuilder MapEntryTemplateEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/entry-templates")
            .WithTags("Entry Templates");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateEntryTemplateRequest>>();
        group.MapPut("/{id:guid}", UpdateAsync).AddEndpointFilter<ValidationFilter<UpdateEntryTemplateRequest>>();
        group.MapDelete("/{id:guid}", DeleteAsync);
        group.MapPost("/{id:guid}/restore", RestoreAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        IEntryTemplateService service,
        CancellationToken cancellationToken)
        => (await service.GetByTrackedActionIdAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetByIdAsync(
        Guid trackedActionId,
        Guid id,
        IEntryTemplateService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateEntryTemplateRequest request,
        IEntryTemplateService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/entry-templates/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateEntryTemplateRequest request,
        IEntryTemplateService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IEntryTemplateService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> RestoreAsync(
        Guid trackedActionId,
        Guid id,
        IEntryTemplateService service,
        CancellationToken cancellationToken)
        => (await service.RestoreAsync(id, cancellationToken)).ToHttpResult();
}
