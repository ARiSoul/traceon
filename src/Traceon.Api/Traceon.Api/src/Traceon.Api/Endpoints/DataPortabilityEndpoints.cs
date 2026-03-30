using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Traceon.Infrastructure.Audit;
using Traceon.Infrastructure.DataPortability;
using Traceon.Infrastructure.Identity;

namespace Traceon.Api.Endpoints;

internal static class DataPortabilityEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static IEndpointRouteBuilder MapDataPortabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/data")
            .RequireAuthorization()
            .WithTags("DataPortability");

        group.MapGet("/export", ExportAsync);
        group.MapPost("/import", ImportAsync).DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> ExportAsync(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        DataPortabilityService portability,
        AuditService audit)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        var data = await portability.ExportAsync(userId, user.Email!);

        await audit.LogAsync(userId, user.Email!, AuditActions.DataExported);

        var json = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        return TypedResults.File(json, "application/json", $"traceon-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }

    private static async Task<IResult> ImportAsync(
        IFormFile file,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        DataPortabilityService portability,
        AuditService audit)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return TypedResults.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return TypedResults.Unauthorized();

        if (file.Length == 0 || file.Length > 50 * 1024 * 1024)
            return TypedResults.BadRequest("File is empty or too large (max 50 MB).");

        UserDataExport data;
        try
        {
            await using var stream = file.OpenReadStream();
            data = await JsonSerializer.DeserializeAsync<UserDataExport>(stream, JsonOptions)
                ?? throw new JsonException("Failed to parse export file.");
        }
        catch (JsonException)
        {
            return TypedResults.BadRequest("Invalid export file format.");
        }

        var result = await portability.ImportAsync(userId, data);

        await audit.LogAsync(userId, user.Email!, AuditActions.DataImported, new
        {
            result.TagsImported,
            result.TagsSkipped,
            result.FieldDefinitionsImported,
            result.FieldDefinitionsSkipped,
            result.ActionsImported,
            result.ActionsRenamed,
            result.EntriesImported,
            InputTags = data.Tags.Count,
            InputFieldDefs = data.FieldDefinitions.Count,
            InputActions = data.TrackedActions.Count,
            InputEntries = data.TrackedActions.Sum(a => a.Entries.Count),
            InputActionFields = data.TrackedActions.Sum(a => a.Fields.Count)
        });

        return TypedResults.Ok(result);
    }
}
