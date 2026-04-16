using System.Net.Http.Json;
using Traceon.Contracts.Analytics;

namespace Traceon.Blazor.Services;

public sealed class ChartVisibilityService(HttpClient http)
{
    public async Task<List<string>> GetHiddenKeysAsync(Guid trackedActionId)
    {
        try
        {
            var response = await http.GetFromJsonAsync<ChartVisibilityResponse>(
                $"/api/tracked-actions/{trackedActionId}/chart-visibility");
            return response?.HiddenKeys ?? [];
        }
        catch
        {
            return [];
        }
    }

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
}
