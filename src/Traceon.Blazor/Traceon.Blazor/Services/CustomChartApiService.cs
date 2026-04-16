using System.Net.Http.Json;
using Traceon.Contracts.CustomCharts;

namespace Traceon.Blazor.Services;

public sealed class CustomChartApiService(HttpClient http)
{
    public async Task<List<CustomChartResponse>> GetByTrackedActionAsync(Guid trackedActionId)
    {
        var charts = await http.GetFromJsonAsync<List<CustomChartResponse>>(
            $"/api/tracked-actions/{trackedActionId}/custom-charts") ?? [];
        return charts.OrderBy(c => c.SortOrder).ToList();
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> CreateAsync(
        Guid trackedActionId, CreateCustomChartRequest request)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/custom-charts", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> UpdateAsync(
        Guid trackedActionId, Guid chartId, UpdateCustomChartRequest request)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/tracked-actions/{trackedActionId}/custom-charts/{chartId}", request);
        return await ToResultAsync(response);
    }

    public async Task<(bool Success, IReadOnlyList<string> Errors)> DeleteAsync(
        Guid trackedActionId, Guid chartId)
    {
        var response = await http.DeleteAsync(
            $"/api/tracked-actions/{trackedActionId}/custom-charts/{chartId}");
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
