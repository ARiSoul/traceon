using System.Security.Claims;

namespace Traceon.Api.Endpoints;

/// <summary>
/// Receives errors caught in the Blazor client (e.g. on a phone, where the browser console
/// is not accessible) so they show up in the API logs.
/// </summary>
internal static class ClientErrorEndpoints
{
    private const int MaxSourceLength = 100;
    private const int MaxMessageLength = 4000;

    public static IEndpointRouteBuilder MapClientErrorEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/client-errors", LogClientError)
            .RequireAuthorization()
            .WithTags("Client Errors");

        return app;
    }

    private static IResult LogClientError(
        ClientErrorRequest request,
        IHttpContextAccessor httpContextAccessor,
        ILogger<Program> logger)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return TypedResults.BadRequest("Message is required.");

        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

        logger.LogWarning(
            "Client error in {Source} for user {UserId} ({UserAgent}): {Message}",
            Truncate(request.Source ?? "Unknown", MaxSourceLength),
            userId,
            userAgent,
            Truncate(request.Message, MaxMessageLength));

        return TypedResults.NoContent();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length > maxLength ? value[..maxLength] + "…" : value;

    private sealed record ClientErrorRequest(string? Source, string Message);
}
