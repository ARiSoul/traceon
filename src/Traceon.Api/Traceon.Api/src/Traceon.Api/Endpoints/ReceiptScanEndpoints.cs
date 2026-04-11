using Traceon.Api.Extensions;
using Traceon.Application.Interfaces;

namespace Traceon.Api.Endpoints;

internal static class ReceiptScanEndpoints
{
    public static RouteGroupBuilder MapReceiptScanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/receipt-scan")
            .WithTags("Receipt Scan")
            .DisableAntiforgery();

        group.MapPost("/", ScanReceiptAsync);

        return group;
    }

    private static async Task<IResult> ScanReceiptAsync(
        IFormFile file,
        IReceiptOcrService ocrService,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return TypedResults.BadRequest("No file uploaded.");

        if (file.Length > 10 * 1024 * 1024)
            return TypedResults.BadRequest("File size must be under 10 MB.");

        await using var stream = file.OpenReadStream();
        var result = await ocrService.ScanReceiptAsync(stream, file.FileName, cancellationToken);

        return result.ToHttpResult();
    }
}
