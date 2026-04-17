using System.Net.Http.Json;
using Traceon.Contracts.Analytics;

namespace Traceon.Blazor.Services;

public sealed class ChartVisibilityService(HttpClient http)
{
    public async Task<ChartVisibilityResponse> GetAsync(Guid trackedActionId)
    {
        try
        {
            var response = await http.GetFromJsonAsync<ChartVisibilityResponse>(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility");
            return response ?? new ChartVisibilityResponse([], []);
        }
        catch
        {
            return new ChartVisibilityResponse([], []);
        }
    }

    public async Task<List<string>> GetHiddenKeysAsync(Guid trackedActionId)
        => (await GetAsync(trackedActionId)).HiddenKeys;

    public async Task SaveHiddenKeysAsync(Guid trackedActionId, IEnumerable<string> hiddenKeys)
    {
        try
        {
            await http.PutAsJsonAsync(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility",
                new UpdateChartVisibilityRequest(hiddenKeys.ToList()));
        }
        catch
        {
            // best effort
        }
    }

    public async Task SaveChartOrderAsync(Guid trackedActionId, IEnumerable<string> chartOrder)
    {
        try
        {
            await http.PutAsJsonAsync(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility/order",
                new UpdateChartOrderRequest(chartOrder.ToList()));
        }
        catch
        {
            // best effort
        }
    }
}
