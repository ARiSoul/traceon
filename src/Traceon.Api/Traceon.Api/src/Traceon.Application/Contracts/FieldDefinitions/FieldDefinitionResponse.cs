using Traceon.Domain.Enums;

namespace Traceon.Application.Contracts.FieldDefinitions;

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
