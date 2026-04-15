using Traceon.Contracts.Enums;

namespace Traceon.Contracts.DropdownValues;

public sealed record UpdateDropdownValueMetadataFieldRequest(
    string Name,
    FieldType Type,
    string? Description = null,
    bool IsRequired = false,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    string? DefaultValue = null,
    string? Unit = null,
    string? DropdownValues = null,
    MetadataDisplayStyle DisplayStyle = MetadataDisplayStyle.Default);
