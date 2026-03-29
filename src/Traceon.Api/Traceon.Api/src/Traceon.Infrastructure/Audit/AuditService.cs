using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Traceon.Infrastructure.Persistence;

namespace Traceon.Infrastructure.Audit;

public sealed class AuditService(
    TraceonDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuditService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task LogAsync(string? userId, string userEmail, string action, object? details = null)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            var ipAddress = GetIpAddress(httpContext);
            var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

            var detailsJson = details is not null
                ? JsonSerializer.Serialize(details, JsonOptions)
                : null;

            var entry = UserAuditLog.Create(userId, userEmail, action, detailsJson, ipAddress, userAgent);
            dbContext.UserAuditLogs.Add(entry);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write audit log: {Action} for {Email}", action, userEmail);
        }
    }

    public async Task<(List<UserAuditLog> Items, int TotalCount)> QueryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        string? action = null,
        string? search = null,
        int skip = 0,
        int take = 20)
    {
        var query = dbContext.UserAuditLogs
            .Where(l => l.UserId == userId)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.OccurredAtUtc >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.OccurredAtUtc <= to.Value);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(l => l.Action == action);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(l => l.Details != null && l.Details.Contains(search)
                || l.IpAddress != null && l.IpAddress.Contains(search)
                || l.UserAgent != null && l.UserAgent.Contains(search));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.OccurredAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }

    private static string? GetIpAddress(HttpContext? httpContext)
    {
        if (httpContext is null) return null;

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',', StringSplitOptions.TrimEntries)[0];

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
