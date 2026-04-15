namespace Traceon.Contracts.DropdownValues;

public sealed record DropdownValueResponse
{
    public required Guid Id { get; init; }
    public required Guid FieldDefinitionId { get; init; }
    public required string Value { get; init; }
    public required int SortOrder { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
    public IReadOnlyList<DropdownValueMetadataValueEntry> Metadata { get; init; } = [];
}