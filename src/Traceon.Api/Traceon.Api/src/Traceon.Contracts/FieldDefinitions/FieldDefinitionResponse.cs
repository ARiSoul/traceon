using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldDefinitions;

public sealed record FieldDefinitionResponse
{
    public required Guid Id { get; init; }
    public required string DefaultName { get; init; }
    public required string? DefaultDescription { get; init; }
    public required FieldType Type { get; init; }
    public required string? DropdownValues { get; init; }
    public required decimal? DefaultMaxValue { get; init; }
    public required decimal? DefaultMinValue { get; init; }
    public required bool DefaultIsRequired { get; init; }
    public required string? DefaultValue { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime? UpdatedAtUtc { get; init; }
}
