using System.Net.Http.Json;
using Traceon.Contracts.ActionFields;

namespace Traceon.Blazor.Services;

public sealed class ActionFieldService(HttpClient http)
{
    public async Task<List<ActionFieldResponse>> GetByTrackedActionAsync(Guid trackedActionId)
    {
        var fields = await http.GetFromJsonAsync<List<ActionFieldResponse>>(
            $"/api/tracked-actions/{trackedActionId}/fields") ?? [];
        return fields.OrderBy(f => f.Order).ToList();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(Guid trackedActionId, CreateActionFieldRequest request)
    {
        var response = await http.PostAsJsonAsync($"/api/tracked-actions/{trackedActionId}/fields", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(Guid trackedActionId, Guid fieldId, UpdateActionFieldRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/tracked-actions/{trackedActionId}/fields/{fieldId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(Guid trackedActionId, Guid fieldId)
    {
        var response = await http.DeleteAsync($"/api/tracked-actions/{trackedActionId}/fields/{fieldId}");
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> RestoreAsync(Guid trackedActionId, Guid fieldId)
    {
        var response = await http.PostAsync($"/api/tracked-actions/{trackedActionId}/fields/{fieldId}/restore", null);
        return await ToResultAsync(response);
    }

    private static async Task<(bool Success, IReadOnlyList<string> Errors)> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, errors);
    }
}
