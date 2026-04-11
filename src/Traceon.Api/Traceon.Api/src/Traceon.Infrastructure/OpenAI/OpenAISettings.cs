namespace Traceon.Infrastructure.OpenAI;

public sealed class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4.1";
    public int MaxTokens { get; set; } = 4096;
}
