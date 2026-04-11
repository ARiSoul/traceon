using Traceon.Api.Extensions;
using Traceon.Application.Services;
using Traceon.Contracts.ReceiptScanDraft;

namespace Traceon.Api.Endpoints;

internal static class ReceiptScanDraftEndpoints
{
    public static RouteGroupBuilder MapReceiptScanDraftEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/receipt-scan-drafts")
            .WithTags("Receipt Scan Drafts");

        group.MapGet("/", GetMyDraftsAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:guid}", UpdateAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);

        return group;
    }

    private static async Task<IResult> GetMyDraftsAsync(
        IReceiptScanDraftService service,
        CancellationToken cancellationToken)
        => (await service.GetMyDraftsAsync(cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IReceiptScanDraftService service,
        CancellationToken cancellationToken)
        => (await service.GetByIdAsync(id, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        CreateReceiptScanDraftRequest request,
        IReceiptScanDraftService service,
        CancellationToken cancellationToken)
        => (await service.CreateAsync(request, cancellationToken))
            .ToCreatedHttpResult(v => $"/api/receipt-scan-drafts/{v.Id}");

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateReceiptScanDraftRequest request,
        IReceiptScanDraftService service,
        CancellationToken cancellationToken)
        => (await service.UpdateAsync(id, request, cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        IReceiptScanDraftService service,
        CancellationToken cancellationToken)
        => (await service.DeleteAsync(id, cancellationToken)).ToHttpResult();
}
