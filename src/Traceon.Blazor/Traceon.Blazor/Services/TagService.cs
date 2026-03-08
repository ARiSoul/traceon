using System.Net.Http.Json;
using Traceon.Contracts.Tags;

namespace Traceon.Blazor.Services;

public sealed class TagService(HttpClient http)
{
    public async Task<List<TagResponse>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<TagResponse>>("/api/tags") ?? [];
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
}
