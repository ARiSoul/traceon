using Traceon.Contracts.Enums;

namespace Traceon.Contracts.DropdownValues;

public sealed record CreateDropdownValueMetadataFieldRequest(
    Guid FieldDefinitionId,
    string Name,
    FieldType Type,
    string? Description = null,
    bool IsRequired = false,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    string? DefaultValue = null,
    string? Unit = null,
    string? DropdownValues = null,
    int SortOrder = 0,
    MetadataDisplayStyle DisplayStyle = MetadataDisplayStyle.Default);
