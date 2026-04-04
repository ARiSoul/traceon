using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed class TemplateService(HttpClient http)
{
    public async Task<IReadOnlyList<TemplatePackResponse>> GetTemplatesAsync()
    {
        try
        {
            var result = await http.GetFromJsonAsync<List<TemplatePackResponse>>("/api/templates");
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool Success, TemplateInstallResult? Result)> InstallAsync(string templateId, string? language = null)
    {
        try
        {
            var url = $"/api/templates/{Uri.EscapeDataString(templateId)}/install";
            if (!string.IsNullOrEmpty(language))
                url += $"?lang={Uri.EscapeDataString(language)}";

            var response = await http.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TemplateInstallResult>();
                return (true, result);
            }
            return (false, null);
        }
        catch
        {
            return (false, null);
        }
    }
}

public sealed record TemplatePackResponse(
    string Id, string NameKey, string DescriptionKey, string Icon, string Color,
    List<TemplateActionSummary> Actions);

public sealed record TemplateActionSummary(string NameKey, int FieldCount);

public sealed record TemplateInstallResult(int TagsCreated, int FieldDefinitionsCreated, int ActionsCreated);
