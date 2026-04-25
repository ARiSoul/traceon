using System.Net.Http.Json;
using Traceon.Contracts.EntryTemplates;

namespace Traceon.Blazor.Services;

public sealed class EntryTemplateService(HttpClient http)
{
    public async Task<List<EntryTemplateResponse>> GetByTrackedActionAsync(Guid trackedActionId)
    {
        var templates = await http.GetFromJsonAsync<List<EntryTemplateResponse>>(
            $"/api/tracked-actions/{trackedActionId}/entry-templates") ?? [];
        return templates;
    }

    public async Task<EntryTemplateResponse?> GetByIdAsync(Guid trackedActionId, Guid templateId)
    {
        return await http.GetFromJsonAsync<EntryTemplateResponse>(
            $"/api/tracked-actions/{trackedActionId}/entry-templates/{templateId}");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        Guid trackedActionId, CreateEntryTemplateRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/entry-templates", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(
        Guid trackedActionId, Guid templateId, UpdateEntryTemplateRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/entry-templates/{templateId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(
        Guid trackedActionId, Guid templateId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/entry-templates/{templateId}");
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> RestoreAsync(
        Guid trackedActionId, Guid templateId)
    {
        var response = await http.PostAsync(
            $"/api/tracked-actions/{trackedActionId}/entry-templates/{templateId}/restore", null);
        return await ToResultAsync(response);
    }

    private static async Task<(bool Success, IReadOnlyList<string> Errors)> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return (true, []);
        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, errors);
    }
}
