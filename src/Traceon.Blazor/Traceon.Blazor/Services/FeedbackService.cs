using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed class FeedbackService(HttpClient http)
{
    public async Task<(bool Success, string? Error)> SendAsync(string category, string message)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/feedback", new { category, message });

            if (response.IsSuccessStatusCode)
                return (true, null);

            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }
        catch
        {
            return (false, null);
        }
    }
}
