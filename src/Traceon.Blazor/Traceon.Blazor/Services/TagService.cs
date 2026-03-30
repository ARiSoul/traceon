using System.Net.Http.Json;
using Traceon.Blazor.Components;
using Traceon.Contracts.Tags;

namespace Traceon.Blazor.Services;

public sealed class TagService(HttpClient http)
{
    public async Task<DataGridResult<TagResponse>> QueryAsync(DataGridRequest request, string[]? searchFields = null)
    {
        var queryString = ODataQueryBuilder.BuildQueryString(request, searchFields);
        using var response = await http.GetAsync($"/api/tags?{queryString}");
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<TagResponse>>() ?? [];
        var totalCount = GetTotalCount(response, items.Count);

        return new DataGridResult<TagResponse>(items, totalCount);
    }

    public async Task<List<TagResponse>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<TagResponse>>("/api/tags") ?? [];
    }

    public async Task<TagResponse?> GetByIdAsync(Guid id)
    {
        return await http.GetFromJsonAsync<TagResponse>($"/api/tags/{id}");
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(CreateTagRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/tags", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(Guid id, UpdateTagRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/tags/{id}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/tags/{id}");
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
