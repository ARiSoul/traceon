using Traceon.Blazor.Components;

namespace Traceon.Blazor.Services;

public static class ODataQueryBuilder
{
    public static string BuildQueryString(DataGridRequest request, string[]? searchFields = null, string? extraFilter = null)
    {
        var parameters = new List<string>
        {
            $"$top={request.PageSize}",
            $"$skip={(request.Page - 1) * request.PageSize}",
            "$count=true"
        };

        if (!string.IsNullOrWhiteSpace(request.SortField))
        {
            var direction = request.SortDescending ? " desc" : "";
            parameters.Add($"$orderby={request.SortField}{direction}");
        }

        var filterClauses = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm) && searchFields is { Length: > 0 })
        {
            var escaped = request.SearchTerm.Replace("'", "''");
            var lower = escaped.ToLowerInvariant();
            var searchClauses = searchFields.Select(f => $"contains(tolower({f}),'{lower}')");
            filterClauses.Add($"({string.Join(" or ", searchClauses)})");
        }

        if (!string.IsNullOrWhiteSpace(extraFilter))
        {
            filterClauses.Add(extraFilter);
        }

        if (filterClauses.Count > 0)
        {
            parameters.Add($"$filter={string.Join(" and ", filterClauses)}");
        }

        return string.Join("&", parameters);
    }
}
