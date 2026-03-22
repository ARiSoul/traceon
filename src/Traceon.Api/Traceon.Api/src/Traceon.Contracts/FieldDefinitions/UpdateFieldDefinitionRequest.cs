using Traceon.Contracts.Enums;

namespace Traceon.Contracts.FieldDefinitions;

public sealed record UpdateFieldDefinitionRequest(
    string DefaultName,
    FieldType Type,
    string? DefaultDescription = null,
    string? DropdownValues = null,
    decimal? DefaultMaxValue = null,
    decimal? DefaultMinValue = null,
    bool DefaultIsRequired = false,
    string? DefaultValue = null,
    string? Unit = null);