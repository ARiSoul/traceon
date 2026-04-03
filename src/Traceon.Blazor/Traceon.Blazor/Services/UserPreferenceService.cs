using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed class UserPreferenceService(HttpClient http)
{
    private sealed record UserPreferencesResponse(string? Theme, string? Language, int DataRetentionDays);
    private sealed record UpdatePreferencesRequest(string? Theme, string? Language, int? DataRetentionDays);

    public async Task<(string? Theme, string? Language, int DataRetentionDays)> GetAsync()
    {
        try
        {
            var response = await http.GetFromJsonAsync<UserPreferencesResponse>("/api/identity/preferences");
            return (response?.Theme, response?.Language, response?.DataRetentionDays ?? 180);
        }
        catch
        {
            return (null, null, 180);
        }
    }

    public async Task SaveAsync(string? theme, string? language, int? dataRetentionDays = null)
    {
        try
        {
            await http.PutAsJsonAsync("/api/identity/preferences", new UpdatePreferencesRequest(theme, language, dataRetentionDays));
        }
        catch
        {
            // Best-effort; localStorage is the fallback
        }
    }
}
