using System.Net.Http.Json;
using Traceon.Blazor.Components;
using Traceon.Contracts.Tags;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Blazor.Services;

public sealed class TrackedActionService(HttpClient http)
{
    public async Task<DataGridResult<TrackedActionResponse>> QueryAsync(DataGridRequest request, string[]? searchFields = null)
    {
        var queryString = ODataQueryBuilder.BuildQueryString(request, searchFields);
        using var response = await http.GetAsync($"/api/tracked-actions?{queryString}");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<TrackedActionResponse>>() ?? [];
        var totalCount = GetTotalCount(response, items.Count);

        return new DataGridResult<TrackedActionResponse>(items, totalCount);
    }

    public async Task<List<TrackedActionResponse>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<TrackedActionResponse>>("/api/tracked-actions") ?? [];
    }

    public async Task<TrackedActionResponse?> GetByIdAsync(Guid id)
    {
        return await http.GetFromJsonAsync<TrackedActionResponse>($"/api/tracked-actions/{id}");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(CreateTrackedActionRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/tracked-actions", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(Guid id, UpdateTrackedActionRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/tracked-actions/{id}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/tracked-actions/{id}");
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> RestoreAsync(Guid id)
    {
        var response = await http.PostAsync($"/api/tracked-actions/{id}/restore", null);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> AddTagAsync(Guid trackedActionId, Guid tagId)
    {
        var response = await http.PostAsync($"/api/tracked-actions/{trackedActionId}/tags/{tagId}", null);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> RemoveTagAsync(Guid trackedActionId, Guid tagId)
    {
        var response = await http.DeleteAsync($"/api/tracked-actions/{trackedActionId}/tags/{tagId}");
        return await ToResultAsync(response);
    }

    public async Task<List<TagResponse>> GetTagsAsync(Guid trackedActionId)
    {
        return await http.GetFromJsonAsync<List<TagResponse>>(
            $"/api/tracked-actions/{trackedActionId}/tags") ?? [];
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> ReorderAsync(List<Guid> orderedIds)
    {
        var response = await http.PutAsJsonAsync("/api/tracked-actions/reorder", orderedIds);
        return await ToResultAsync(response);
    }

    private static async Task<(bool Success, IReadOnlyList<string> Errors)> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, errors);
    }

    private static int GetTotalCount(HttpResponseMessage response, int fallbackCount)
    {
        if (response.Headers.TryGetValues("X-Total-Count", out var values)
            && int.TryParse(values.FirstOrDefault(), out var count))
            return count;

        return fallbackCount;
    }
}
