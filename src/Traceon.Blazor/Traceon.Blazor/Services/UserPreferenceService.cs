using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed class UserPreferenceService(HttpClient http)
{
    private sealed record UserPreferencesResponse(string? Theme, string? Language);
    private sealed record UpdatePreferencesRequest(string? Theme, string? Language);

    public async Task<(string? Theme, string? Language)> GetAsync()
    {
        try
        {
            var response = await http.GetFromJsonAsync<UserPreferencesResponse>("/api/identity/preferences");
            return (response?.Theme, response?.Language);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task SaveAsync(string? theme, string? language)
    {
        try
        {
            await http.PutAsJsonAsync("/api/identity/preferences", new UpdatePreferencesRequest(theme, language));
        }
        catch
        {
            // Best-effort; localStorage is the fallback
        }
    }
}
