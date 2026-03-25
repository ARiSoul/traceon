namespace Traceon.Contracts.ActionFields;

public sealed record UpdateActionFieldRequest(
    string Name,
    string? Description = null,
    decimal? MaxValue = null,
    decimal? MinValue = null,
    bool IsRequired = false,
    string? DefaultValue = null,
    string? Unit = null,
    int? Order = null);
