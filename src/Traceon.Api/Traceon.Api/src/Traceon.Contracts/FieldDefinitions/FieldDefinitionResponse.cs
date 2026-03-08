using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldDefinitions;

public sealed record FieldDefinitionResponse(
    Guid Id,
    string DefaultName,
    string? DefaultDescription,
    FieldType Type,
    string? DropdownValues,
    decimal? DefaultMaxValue,
    decimal? DefaultMinValue,
    bool DefaultIsRequired,
    string? DefaultValue,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
