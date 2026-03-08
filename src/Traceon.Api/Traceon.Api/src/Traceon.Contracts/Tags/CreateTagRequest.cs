namespace Traceon.Contracts.Tags;

public sealed record CreateTagRequest(
    string Name,
    string? Description = null,
    string Color = "#000000");
