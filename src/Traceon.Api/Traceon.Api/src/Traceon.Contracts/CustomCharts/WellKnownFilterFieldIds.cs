namespace Traceon.Contracts.CustomCharts;

/// <summary>
/// Reserved <see cref="FilterConditionDto.FieldId"/> values that target entry-level properties
/// (e.g. <see cref="ActionEntries.ActionEntryResponse.Notes"/>) rather than user-defined ActionFields.
/// </summary>
public static class WellKnownFilterFieldIds
{
    public static readonly Guid Notes = new("00000000-0000-0000-0000-000000000001");

    public static bool IsWellKnown(Guid fieldId) => fieldId == Notes;
}
