using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Traceon.Contracts.Analytics;

namespace Traceon.Blazor.Services;

public sealed class ChartVisibilityService(HttpClient http)
{
    public async Task<ChartVisibilityResponse> GetAsync(Guid trackedActionId)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/tracked-actions/{trackedActionId}/chart-visibility");
            request.SetBrowserRequestCache(BrowserRequestCache.NoStore);

            using var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ChartVisibilityResponse>()
                ?? new ChartVisibilityResponse([], []);

            Console.WriteLine($"[ChartVisibility] GET {trackedActionId}: hidden={payload.HiddenKeys.Count}, order=[{string.Join(",", payload.ChartOrder)}]");
            return payload;
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
            var orderList = chartOrder.ToList();
            var response = await http.PutAsJsonAsync(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility/order",
                new UpdateChartOrderRequest(orderList));
            Console.WriteLine($"[ChartVisibility] PUT order {trackedActionId} status={(int)response.StatusCode}: [{string.Join(",", orderList)}]");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ChartVisibility] PUT chart-order failed: {ex.Message}");
            return false;
        }
    }
}
