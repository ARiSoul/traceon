using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed class DataPortabilityService(HttpClient http)
{
    public string ExportUrl => $"{http.BaseAddress}api/data/export";

    public async Task<Stream> ExportAsync()
    {
        var response = await http.GetAsync("/api/data/export");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<(bool Success, ImportResult? Result, string? Error)> ImportAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        content.Add(streamContent, "file", fileName);

        var response = await http.PostAsync("/api/data/import", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, null, error);
        }

        var result = await response.Content.ReadFromJsonAsync<ImportResult>();
        return (true, result, null);
    }
}

public sealed record ImportResult(
    int TagsImported, int TagsSkipped,
    int FieldDefinitionsImported, int FieldDefinitionsSkipped,
    int ActionsImported, int ActionsRenamed, int EntriesImported);
