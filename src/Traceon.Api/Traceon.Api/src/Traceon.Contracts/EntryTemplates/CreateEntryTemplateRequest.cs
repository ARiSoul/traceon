namespace Traceon.Contracts.EntryTemplates;

public sealed record CreateEntryTemplateRequest(
    string Name,
    string? Notes = null,
    List<EntryTemplateFieldValue>? FieldValues = null);

public sealed record EntryTemplateFieldValue(
    Guid ActionFieldId,
    string? Value);
