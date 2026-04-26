namespace Traceon.Contracts.EntryTemplates;

public sealed record CreateEntryTemplateRequest(
    string Name,
    string? Notes = null,
    List<EntryTemplateFieldInput>? FieldValues = null);

public sealed record EntryTemplateFieldInput(
    Guid ActionFieldId,
    List<string>? Values)
{
    public string? Value => Values is { Count: > 0 } ? Values[0] : null;
}
