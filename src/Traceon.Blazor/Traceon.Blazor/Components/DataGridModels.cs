using Microsoft.AspNetCore.Components;

namespace Traceon.Blazor.Components;

public enum ColumnSummaryType
{
    Sum,
    Avg,
    Min,
    Max,
    Count
}

public sealed class DataGridColumn<TItem>
{
    public required string Title { get; init; }
    public required Func<TItem, object?> Selector { get; init; }
    public string? SortField { get; init; }
    public string? Format { get; init; }
    public RenderFragment<TItem>? Template { get; init; }

    /// <summary>When true the column is hidden in the mobile card layout.</summary>
    public bool HideOnMobile { get; init; }

    /// <summary>When true the column value is used as the card title on mobile instead of showing a label/value pair.</summary>
    public bool IsMobileTitle { get; init; }
}

public sealed record DataGridRequest(
    int Page,
    int PageSize,
    string? SortField,
    bool SortDescending,
    string? SearchTerm);

public sealed record DataGridResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount);

/// <summary>State persisted to localStorage for each grid instance.</summary>
public sealed class DataGridPersistedState
{
    public string? SortField { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
    public int? PageSize { get; set; }
    public Dictionary<string, List<ColumnSummaryType>>? Summaries { get; set; }
}
