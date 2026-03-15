using Traceon.Contracts.Enums;

namespace Traceon.Contracts.ActionFields;

public sealed record ActionFieldResponse
{
    public required Guid Id { get; init; }
    public required Guid TrackedActionId { get; init; }
    public required Guid FieldDefinitionId { get; init; }
    public required FieldType FieldType { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required decimal? MaxValue { get; init; }
    public required decimal? MinValue { get; init; }
    public required bool IsRequired { get; init; }
    public required string? DefaultValue { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}