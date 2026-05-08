using Traceon.Contracts.CustomCharts;
using Traceon.Contracts.Enums;

namespace Traceon.Blazor.Services;

/// <summary>
/// Translates a <see cref="FilterGroupDto"/> into an OData $filter expression
/// for the entries query API. Honors all <see cref="FilterOperator"/> values and
/// the <see cref="WellKnownFilterFieldIds.Notes"/> sentinel. Output is always
/// parenthesised so callers can safely AND/OR it with other clauses.
/// </summary>
/// <remarks>
/// Numeric and date comparisons on ActionField values are translated as string
/// comparisons (Values are stored as strings server-side); unpadded numbers may
/// order differently than the chart's client-side decimal-aware evaluator.
/// </remarks>
public static class FilterGroupOData
{
    public static string? ToFilter(FilterGroupDto? group)
    {
        if (group is null) return null;
        var inner = WriteGroup(group);
        return string.IsNullOrEmpty(inner) ? null : $"({inner})";
    }

    private static string WriteGroup(FilterGroupDto group)
    {
        var parts = new List<string>();

        if (group.Conditions is not null)
        {
            foreach (var c in group.Conditions)
            {
                var s = WriteCondition(c);
                if (!string.IsNullOrEmpty(s)) parts.Add(s);
            }
        }

        if (group.Groups is not null)
        {
            foreach (var sub in group.Groups)
            {
                var inner = WriteGroup(sub);
                if (!string.IsNullOrEmpty(inner)) parts.Add($"({inner})");
            }
        }

        if (parts.Count == 0) return string.Empty;
        var joiner = group.Logic == FilterLogic.Or ? " or " : " and ";
        return parts.Count == 1 ? parts[0] : string.Join(joiner, parts);
    }

    private static string WriteCondition(FilterConditionDto c)
        => c.FieldId == WellKnownFilterFieldIds.Notes
            ? WriteNotesCondition(c)
            : WriteFieldCondition(c);

    private static string WriteNotesCondition(FilterConditionDto c)
    {
        var raw = c.Value ?? string.Empty;
        var v = Esc(raw);
        var lower = Lower(raw);

        return c.Operator switch
        {
            FilterOperator.Equals => $"Notes eq '{v}'",
            FilterOperator.NotEquals => $"(Notes eq null or Notes ne '{v}')",
            FilterOperator.Contains => $"(Notes ne null and contains(tolower(Notes),'{lower}'))",
            FilterOperator.NotContains => $"(Notes eq null or not contains(tolower(Notes),'{lower}'))",
            FilterOperator.StartsWith => $"(Notes ne null and startswith(tolower(Notes),'{lower}'))",
            FilterOperator.EndsWith => $"(Notes ne null and endswith(tolower(Notes),'{lower}'))",
            FilterOperator.IsEmpty => "(Notes eq null or Notes eq '')",
            FilterOperator.IsNotEmpty => "(Notes ne null and Notes ne '')",
            _ => "true"
        };
    }

    private static string WriteFieldCondition(FilterConditionDto c)
    {
        var fid = c.FieldId;

        string Any(string predicate)
            => $"FieldValues/any(fv: fv/ActionFieldId eq {fid} and fv/Values/any(v: {predicate}))";

        var raw = c.Value ?? string.Empty;
        var rawTo = c.ValueTo ?? string.Empty;

        return c.Operator switch
        {
            FilterOperator.Equals             => Any($"v eq '{Esc(raw)}'"),
            FilterOperator.NotEquals          => $"not ({Any($"v eq '{Esc(raw)}'")})",
            FilterOperator.Contains           => Any($"contains(tolower(v),'{Lower(raw)}')"),
            FilterOperator.NotContains        => $"not ({Any($"contains(tolower(v),'{Lower(raw)}')")})",
            FilterOperator.StartsWith         => Any($"startswith(tolower(v),'{Lower(raw)}')"),
            FilterOperator.EndsWith           => Any($"endswith(tolower(v),'{Lower(raw)}')"),
            FilterOperator.GreaterThan        => Any($"v gt '{Esc(raw)}'"),
            FilterOperator.GreaterThanOrEqual => Any($"v ge '{Esc(raw)}'"),
            FilterOperator.LessThan           => Any($"v lt '{Esc(raw)}'"),
            FilterOperator.LessThanOrEqual    => Any($"v le '{Esc(raw)}'"),
            FilterOperator.Between            => Any($"v ge '{Esc(raw)}' and v le '{Esc(rawTo)}'"),
            FilterOperator.In                 => Any(BuildOrEquals(c.Value)),
            FilterOperator.NotIn              => $"not ({Any(BuildOrEquals(c.Value))})",
            FilterOperator.IsEmpty =>
                $"not (FieldValues/any(fv: fv/ActionFieldId eq {fid} and fv/Values/any(v: v ne null and v ne '')))",
            FilterOperator.IsNotEmpty =>
                $"FieldValues/any(fv: fv/ActionFieldId eq {fid} and fv/Values/any(v: v ne null and v ne ''))",
            _ => "true"
        };
    }

    private static string BuildOrEquals(string? piped)
    {
        if (string.IsNullOrWhiteSpace(piped)) return "false";
        var parts = piped.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 0
            ? "false"
            : string.Join(" or ", parts.Select(p => $"v eq '{Esc(p)}'"));
    }

    private static string Esc(string s) => s.Replace("'", "''");
    private static string Lower(string s) => s.ToLowerInvariant().Replace("'", "''");
}
