namespace Traceon.Contracts.ActionFields;

public sealed record CreateActionFieldRequest(
    Guid FieldDefinitionId,
    string Name,
    string? Description = null,
    decimal? MaxValue = null,
    decimal? MinValue = null,
    bool IsRequired = false,
    string? DefaultValue = null);
