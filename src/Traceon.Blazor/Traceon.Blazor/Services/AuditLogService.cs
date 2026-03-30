using System.Net.Http.Json;

namespace Traceon.Blazor.Services;

public sealed class AuditLogService(HttpClient http)
{
    public async Task<AuditLogPageResponse> GetLogsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? action = null,
        string? search = null,
        int skip = 0,
        int take = 20)
    {
        var query = new List<string>();

        if (from.HasValue) query.Add($"from={from.Value:O}");
        if (to.HasValue) query.Add($"to={to.Value:O}");
        if (!string.IsNullOrEmpty(action)) query.Add($"action={Uri.EscapeDataString(action)}");
        if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        query.Add($"skip={skip}");
        query.Add($"take={take}");

        var url = $"/api/identity/audit-logs?{string.Join("&", query)}";

        var response = await http.GetFromJsonAsync<AuditLogPageResponse>(url);
        return response ?? new AuditLogPageResponse([], 0);
    }
}

public sealed record AuditLogResponse(Guid Id, string Action, string? Details, string? IpAddress, string? UserAgent, DateTime OccurredAtUtc);

public sealed record AuditLogPageResponse(IReadOnlyList<AuditLogResponse> Items, int TotalCount);
