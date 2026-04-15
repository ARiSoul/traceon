using Traceon.Contracts.Enums;

namespace Traceon.Contracts.DropdownValues;

public sealed record DropdownValueMetadataFieldResponse
{
    public required Guid Id { get; init; }
    public required Guid FieldDefinitionId { get; init; }
    public required string Name { get; init; }
    public required FieldType Type { get; init; }
    public required string? Description { get; init; }
    public required bool IsRequired { get; init; }
    public required decimal? MinValue { get; init; }
    public required decimal? MaxValue { get; init; }
    public required string? DefaultValue { get; init; }
    public required string? Unit { get; init; }
    public required string? DropdownValues { get; init; }
    public required int SortOrder { get; init; }
    public required MetadataDisplayStyle DisplayStyle { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
