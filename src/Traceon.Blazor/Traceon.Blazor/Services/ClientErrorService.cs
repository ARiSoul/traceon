using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

/// <summary>
/// Sends client-side errors to the API logs, so failures on phones (where the browser
/// console is not accessible) can be diagnosed with <c>docker compose logs api</c>.
/// </summary>
public sealed class ClientErrorService(HttpClient http)
{
    /// <summary>Best effort: never throws, so it is safe to call from a catch block.</summary>
    public async Task ReportAsync(string source, string message)
    {
        try
        {
            await http.PostAsJsonAsync("/api/client-errors", new { source, message });
        }
        catch
        {
            // Reporting must never surface a second error to the user
        }
    }
}
