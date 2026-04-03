using System.Net.Http.Json;
using Traceon.Blazor.Components;
using Traceon.Contracts.ActionEntries;

namespace Traceon.Blazor.Services;

public sealed class ActionEntryService(HttpClient http)
{
    public async Task<DataGridResult<ActionEntryResponse>> QueryAsync(
        DataGridRequest request, string[]? searchFields = null, string? extraFilter = null)
    {
        var queryString = ODataQueryBuilder.BuildQueryString(request, searchFields, extraFilter);
        using var response = await http.GetAsync($"/api/entries?{queryString}");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<ActionEntryResponse>>() ?? [];

        var totalCount = items.Count;
        if (response.Headers.TryGetValues("X-Total-Count", out var values) &&
            int.TryParse(values.FirstOrDefault(), out var count))
        {
            totalCount = count;
        }

        return new DataGridResult<ActionEntryResponse>(items, totalCount);
    }

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

    public async Task<(bool Success, IReadOnlyList<string> Errors)> RestoreAsync(
        Guid trackedActionId, Guid entryId)
    {
        var response = await http.PostAsync(
            $"/api/tracked-actions/{trackedActionId}/entries/{entryId}/restore", null);
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
