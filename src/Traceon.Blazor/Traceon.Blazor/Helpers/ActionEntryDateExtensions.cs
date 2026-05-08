using Traceon.Contracts.ActionEntries;

namespace Traceon.Blazor.Helpers;

internal static class ActionEntryDateExtensions
{
    // Returns the entry's local-time wall-clock value but with Kind=Utc — the "fake-UTC"
    // analytics convention. This way the value serializes as "…T00:00:00Z" and ApexCharts
    // (default datetimeUTC=true) renders the dot at the local-day tick, and the tooltip
    // formats the local date. DateTime equality/grouping is by Ticks so the Kind switch is
    // safe for use as a dictionary/grouping key. For human-facing display use ToLocalTime()
    // (or just keep the wall-clock components, since ticks already match local).
    public static DateTime OccurredLocal(this ActionEntryResponse e)
        => DateTime.SpecifyKind(e.OccurredAtUtc.ToLocalTime(), DateTimeKind.Utc);

    public static DateTime OccurredLocalDate(this ActionEntryResponse e)
        => DateTime.SpecifyKind(e.OccurredAtUtc.ToLocalTime().Date, DateTimeKind.Utc);
}
