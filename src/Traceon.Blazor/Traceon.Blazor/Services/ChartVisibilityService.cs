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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ChartVisibility] GET failed: {ex.Message}");
            return new ChartVisibilityResponse([], []);
        }
    }

    public async Task<List<string>> GetHiddenKeysAsync(Guid trackedActionId)
        => (await GetAsync(trackedActionId)).HiddenKeys;

    public async Task<bool> SaveHiddenKeysAsync(Guid trackedActionId, IEnumerable<string> hiddenKeys)
    {
        try
        {
            var response = await http.PutAsJsonAsync(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility",
                new UpdateChartVisibilityRequest(hiddenKeys.ToList()));
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ChartVisibility] PUT hidden-keys failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SaveChartOrderAsync(Guid trackedActionId, IEnumerable<string> chartOrder)
    {
        try
        {
            var response = await http.PutAsJsonAsync(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility/order",
                new UpdateChartOrderRequest(chartOrder.ToList()));
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ChartVisibility] PUT chart-order failed: {ex.Message}");
            return false;
        }
    }
}
