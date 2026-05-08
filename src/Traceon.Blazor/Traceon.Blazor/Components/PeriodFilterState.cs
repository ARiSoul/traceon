namespace Traceon.Blazor.Components;

public sealed class PeriodFilterState
{
    public string Mode { get; set; } = "all";
    public int Value { get; set; } = 30;
    public DateTime? CustomFrom { get; set; }
    public DateTime? CustomTo { get; set; }
    public DateTime? SelectedMonth { get; set; }
    public DateTime? SinceDate { get; set; }
    public string DateField { get; set; } = "OccurredAtUtc";

    public bool IsActive(string defaultMode = "all") =>
        !string.Equals(Mode, defaultMode, StringComparison.Ordinal);

    public (DateTime? From, DateTime? To) Resolve() => Mode switch
    {
        "days" => (DateTime.UtcNow.AddDays(-Value), null),
        "months" => (DateTime.UtcNow.AddMonths(-Value), null),
        "years" => (DateTime.UtcNow.AddYears(-Value), null),
        "month" when SelectedMonth.HasValue
            => (LocalDayStartAsUtc(SelectedMonth.Value), LocalDayStartAsUtc(SelectedMonth.Value.AddMonths(1))),
        "since" when SinceDate.HasValue
            => (LocalDayStartAsUtc(SinceDate.Value), null),
        "custom" => (
            CustomFrom.HasValue ? LocalDayStartAsUtc(CustomFrom.Value) : null,
            CustomTo.HasValue ? LocalDayStartAsUtc(CustomTo.Value.AddDays(1)) : null),
        _ => (null, null)
    };

    // Date-input modes carry a local-day boundary (the user picked a calendar day in their TZ).
    // Convert to a true UTC instant so OData filters with a literal Z suffix match correctly.
    private static DateTime LocalDayStartAsUtc(DateTime d)
        => DateTime.SpecifyKind(d.Date, DateTimeKind.Local).ToUniversalTime();
}
