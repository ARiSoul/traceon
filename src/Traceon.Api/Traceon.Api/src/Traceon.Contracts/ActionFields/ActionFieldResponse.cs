using Traceon.Contracts.Enums;

namespace Traceon.Contracts.ActionFields;

public sealed record ActionFieldResponse(
    Guid Id,
    Guid TrackedActionId,
    Guid FieldDefinitionId,
    FieldType FieldType,
    string Name,
    string? Description,
    decimal? MaxValue,
    decimal? MinValue,
    bool IsRequired,
    string? DefaultValue,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
