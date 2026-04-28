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
        "month" when SelectedMonth.HasValue => (SelectedMonth.Value, SelectedMonth.Value.AddMonths(1)),
        "since" when SinceDate.HasValue => (SinceDate.Value, null),
        "custom" => (CustomFrom, CustomTo?.AddDays(1)),
        _ => (null, null)
    };
}
