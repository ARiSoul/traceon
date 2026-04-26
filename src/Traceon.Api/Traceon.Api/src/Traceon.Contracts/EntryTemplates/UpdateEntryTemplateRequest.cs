namespace Traceon.Contracts.EntryTemplates;

public sealed record UpdateEntryTemplateRequest(
    string Name,
    string? Notes = null,
    List<EntryTemplateFieldInput>? FieldValues = null);
