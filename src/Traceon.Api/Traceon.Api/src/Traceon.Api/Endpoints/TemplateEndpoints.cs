using System.Security.Claims;
using Traceon.Infrastructure.Onboarding;

namespace Traceon.Api.Endpoints;

internal static class TemplateEndpoints
{
    public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/templates")
            .RequireAuthorization()
            .WithTags("Templates");

        group.MapGet("/", GetTemplatesAsync);
        group.MapPost("/{id}/install", InstallTemplateAsync);

        return app;
    }

    private static IResult GetTemplatesAsync()
    {
        var templates = TemplatePackCatalog.All.Select(p => new TemplatePackResponse(
            p.Id, p.NameKey, p.DescriptionKey, p.Icon, p.Color,
            p.Actions.Select(a => new TemplateActionSummary(a.NameKey, a.Fields.Count)).ToList()
        )).ToList();

        return TypedResults.Ok(templates);
    }

    private static async Task<IResult> InstallTemplateAsync(
        string id,
        IHttpContextAccessor httpContextAccessor,
        TemplateInstallService installService)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var pack = TemplatePackCatalog.GetById(id);
        if (pack is null) return TypedResults.NotFound();

        var result = await installService.InstallAsync(userId, pack);
        return TypedResults.Ok(result);
    }

    private sealed record TemplatePackResponse(
        string Id, string NameKey, string DescriptionKey, string Icon, string Color,
        List<TemplateActionSummary> Actions);

    private sealed record TemplateActionSummary(string NameKey, int FieldCount);
}
