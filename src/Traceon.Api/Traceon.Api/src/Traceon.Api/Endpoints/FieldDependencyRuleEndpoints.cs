using Traceon.Api.Extensions;
using Traceon.Contracts.FieldDependencyRules;
using Traceon.Application.Services;

namespace Traceon.Api.Endpoints;

internal static class FieldDependencyRuleEndpoints
{
    public static RouteGroupBuilder MapFieldDependencyRuleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tracked-actions/{trackedActionId:guid}/dependency-rules")
            .WithTags("Field Dependency Rules");

        group.MapGet("/", GetByTrackedActionIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static async Task<IResult> GetByTrackedActionIdAsync(
        Guid trackedActionId,
        IFieldDependencyRuleService service,
        CancellationToken cancellationToken)
        => (await service.GetByTrackedActionIdAsync(trackedActionId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        Guid trackedActionId,
        CreateFieldDependencyRuleRequest request,
        IFieldDependencyRuleService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(trackedActionId, request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/tracked-actions/{trackedActionId}/dependency-rules/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid trackedActionId,
        Guid id,
        UpdateFieldDependencyRuleRequest request,
        IFieldDependencyRuleService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid trackedActionId,
        Guid id,
        IFieldDependencyRuleService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();
}
