namespace Traceon.Application.Contracts.Tags;

public sealed record UpdateTagRequest(
    string Name,
    string? Description = null,
    string Color = "#000000");
