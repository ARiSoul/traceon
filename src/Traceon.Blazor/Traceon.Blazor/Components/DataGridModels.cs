using Microsoft.AspNetCore.Components;

namespace Traceon.Blazor.Components;

public sealed class DataGridColumn<TItem>
{
    public required string Title { get; init; }
    public required Func<TItem, object?> Selector { get; init; }
    public string? SortField { get; init; }
    public string? Format { get; init; }
    public RenderFragment<TItem>? Template { get; init; }
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
