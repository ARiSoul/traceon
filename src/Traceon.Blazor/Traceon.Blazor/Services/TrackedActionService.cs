using System.Net.Http.Json;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Blazor.Services;

public sealed class TrackedActionService(HttpClient http)
{
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

    private static async Task<(bool Success, IReadOnlyList<string> Errors)> ToResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return (true, []);

        var errors = await ApiErrorParser.ExtractErrorsAsync(response);
        return (false, errors);
    }
}
