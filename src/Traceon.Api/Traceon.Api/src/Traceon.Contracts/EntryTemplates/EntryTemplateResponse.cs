namespace Traceon.Contracts.EntryTemplates;

public sealed record EntryTemplateResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required string Name { get; init; }
    public required string? Notes { get; init; }
    public required List<EntryTemplateFieldResponse> FieldValues { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}

public sealed record EntryTemplateFieldResponse
{
    public required Guid Id { get; init; }
    public required Guid ActionFieldId { get; init; }
    public required string? Value { get; init; }
}
