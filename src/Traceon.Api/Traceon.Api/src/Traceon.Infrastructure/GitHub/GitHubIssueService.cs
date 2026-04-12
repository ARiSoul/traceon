using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Traceon.Infrastructure.GitHub;

public sealed class GitHubIssueService(HttpClient http, GitHubSettings settings, ILogger<GitHubIssueService> logger)
{
    private static readonly Dictionary<string, string> LabelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bug"] = "bug",
        ["Feature"] = "enhancement",
        ["General"] = "feedback"
    };

    public async Task<string?> CreateIssueAsync(string category, string message, string userEmail)
    {
        if (string.IsNullOrEmpty(settings.Token))
        {
            logger.LogWarning("GitHub token not configured; skipping issue creation.");
            return null;
        }

        var label = LabelMap.GetValueOrDefault(category, "feedback");
        var labels = new HashSet<string> { label, "feedback" };

        // Ensure labels exist (GitHub rejects issues referencing non-existent labels)
        foreach (var l in labels)
            await EnsureLabelAsync(l);

        var title = $"[{category}] Feedback from user";
        var body = $"**Category:** {category}\n**From:** {userEmail}\n\n---\n\n{message}";

        var payload = JsonSerializer.Serialize(new
        {
            title,
            body,
            labels = labels.ToArray()
        });

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.github.com/repos/{settings.Owner}/{settings.Repo}/issues");
        SetHeaders(request);
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        try
        {
            var response = await http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var issueUrl = doc.RootElement.GetProperty("html_url").GetString();
                logger.LogInformation("GitHub issue created: {Url}", issueUrl);
                return issueUrl;
            }

            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("GitHub API returned {Status}: {Error}", response.StatusCode, error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create GitHub issue for feedback");
        }

        return null;
    }

    private async Task EnsureLabelAsync(string labelName)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { name = labelName });
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://api.github.com/repos/{settings.Owner}/{settings.Repo}/labels");
            SetHeaders(request);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await http.SendAsync(request);
            // 201 = created, 422 = already exists — both are fine
            if (!response.IsSuccessStatusCode && (int)response.StatusCode != 422)
                logger.LogWarning("Failed to ensure label '{Label}': {Status}", labelName, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to ensure label '{Label}'", labelName);
        }
    }

    private void SetHeaders(HttpRequestMessage request)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.Token);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Traceon", "1.0"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }
}
