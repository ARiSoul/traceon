using System.Text.Json;

namespace Traceon.Blazor.Services;

public static class ApiErrorParser
{
    public static async Task<IReadOnlyList<string>> ExtractErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(body) && !string.IsNullOrWhiteSpace(response.ReasonPhrase))
            return [response.ReasonPhrase];
        else if (string.IsNullOrWhiteSpace(body))
            return ["An unexpected error occurred."];

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Plain string response: BadRequest("message")
            if (root.ValueKind == JsonValueKind.String)
            {
                var text = root.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return [text];
            }

            var messages = new List<string>();

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in errors.EnumerateObject())
                {
                    foreach (var msg in field.Value.EnumerateArray())
                    {
                        var text = msg.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                            messages.Add(text);
                    }
                }
            }

            if (messages.Count > 0)
                return messages;

            if (root.TryGetProperty("title", out var title))
            {
                var text = title.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return [text];
            }

            if (root.TryGetProperty("detail", out var detail))
            {
                var text = detail.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                    return [text];
            }
        }
        catch (JsonException)
        {
            // Not JSON – fall through
        }

        return ["An unexpected error occurred."];
    }
}
