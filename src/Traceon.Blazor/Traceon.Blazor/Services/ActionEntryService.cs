using System.Net.Http.Json;
using Traceon.Contracts.ActionEntries;

namespace Traceon.Blazor.Services;

public sealed class ActionEntryService(HttpClient http)
{
    public async Task<ActionEntryResponse?> GetByIdAsync(Guid trackedActionId, Guid entryId)
    {
        return await http.GetFromJsonAsync<ActionEntryResponse>(
            $"/api/tracked-actions/{trackedActionId}/entries/{entryId}");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        Guid trackedActionId, CreateActionEntryRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/entries", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(
        Guid trackedActionId, Guid entryId, UpdateActionEntryRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/entries/{entryId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(
        Guid trackedActionId, Guid entryId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/entries/{entryId}");
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
