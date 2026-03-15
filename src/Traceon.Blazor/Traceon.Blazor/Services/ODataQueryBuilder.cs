using Traceon.Blazor.Components;

namespace Traceon.Blazor.Services;

public static class ODataQueryBuilder
{
    public static string BuildQueryString(DataGridRequest request, string[]? searchFields = null)
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

        if (!string.IsNullOrWhiteSpace(request.SearchTerm) && searchFields is { Length: > 0 })
        {
            var escaped = request.SearchTerm.Replace("'", "''");
            var lower = escaped.ToLowerInvariant();
            var filterClauses = searchFields.Select(f => $"contains(tolower({f}),'{lower}')");
            parameters.Add($"$filter={string.Join(" or ", filterClauses)}");
        }

        return string.Join("&", parameters);
    }
}
