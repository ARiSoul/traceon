using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traceon.Domain.Entities;

namespace Traceon.Infrastructure.Persistence;

internal sealed class PurgeDeletedDataService(
    IServiceScopeFactory scopeFactory,
    IOptions<PurgeSettings> options,
    ILogger<PurgeDeletedDataService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(options.Value.IntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);

            try
            {
                await PurgeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.PurgeFailed(ex);
            }
        }
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TraceonDbContext>();

        var defaultRetention = options.Value.DefaultRetentionDays;

        var users = await context.Users
            .Select(u => new { u.Id, u.DataRetentionDays })
            .ToListAsync(cancellationToken);

        var totalPurged = 0;

        foreach (var user in users)
        {
            var retentionDays = user.DataRetentionDays > 0 ? user.DataRetentionDays : defaultRetention;
            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

            var purged = await PurgeUserDataAsync(context, user.Id, cutoff, cancellationToken);
            totalPurged += purged;
        }

        if (totalPurged > 0)
            logger.PurgeCompleted(totalPurged);
    }

    private static async Task<int> PurgeUserDataAsync(
        TraceonDbContext context, string userId, DateTime cutoff, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var purged = 0;

            // 1. Expired ActionField IDs (independently deleted)
            var expiredFieldIds = await context.ActionFields
                .IgnoreQueryFilters()
                .Where(af => af.IsDeleted && af.DeletedAtUtc <= cutoff
                    && context.TrackedActions.IgnoreQueryFilters().Any(a => a.Id == af.TrackedActionId && a.UserId == userId))
                .Select(af => af.Id)
                .ToListAsync(cancellationToken);

            // 2. Expired TrackedAction IDs
            var expiredActionIds = await context.TrackedActions
                .IgnoreQueryFilters()
                .Where(a => a.UserId == userId && a.IsDeleted && a.DeletedAtUtc <= cutoff)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            // 3. All ActionField IDs to purge (independently deleted + children of deleted actions)
            var allFieldIdsToPurge = await context.ActionFields
                .IgnoreQueryFilters()
                .Where(af => expiredFieldIds.Contains(af.Id) || expiredActionIds.Contains(af.TrackedActionId))
                .Select(af => af.Id)
                .ToListAsync(cancellationToken);

            // 4. Expired ActionEntry IDs (independently deleted)
            var expiredEntryIds = await context.ActionEntries
                .IgnoreQueryFilters()
                .Where(e => e.IsDeleted && e.DeletedAtUtc <= cutoff
                    && expiredActionIds.Contains(e.TrackedActionId) == false
                    && context.TrackedActions.IgnoreQueryFilters().Any(a => a.Id == e.TrackedActionId && a.UserId == userId))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            // 5. All ActionEntry IDs to purge (independently deleted + children of deleted actions)
            var allEntryIdsToPurge = await context.ActionEntries
                .IgnoreQueryFilters()
                .Where(e => expiredEntryIds.Contains(e.Id) || expiredActionIds.Contains(e.TrackedActionId))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            // --- Delete in FK-safe order ---

            // ActionEntryFields (reference both ActionField and ActionEntry)
            if (allFieldIdsToPurge.Count > 0 || allEntryIdsToPurge.Count > 0)
            {
                purged += await context.ActionEntryFields
                    .IgnoreQueryFilters()
                    .Where(ef => allFieldIdsToPurge.Contains(ef.ActionFieldId) || allEntryIdsToPurge.Contains(ef.ActionEntryId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // FieldAnalyticsRules (reference ActionField and TrackedAction)
            if (allFieldIdsToPurge.Count > 0 || expiredActionIds.Count > 0)
            {
                purged += await context.FieldAnalyticsRules
                    .IgnoreQueryFilters()
                    .Where(r => expiredActionIds.Contains(r.TrackedActionId)
                        || allFieldIdsToPurge.Contains(r.MeasureFieldId)
                        || allFieldIdsToPurge.Contains(r.GroupByFieldId)
                        || (r.FilterFieldId != null && allFieldIdsToPurge.Contains(r.FilterFieldId.Value)))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // ActionFields
            if (allFieldIdsToPurge.Count > 0)
            {
                purged += await context.ActionFields
                    .IgnoreQueryFilters()
                    .Where(af => allFieldIdsToPurge.Contains(af.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // ActionEntries
            if (allEntryIdsToPurge.Count > 0)
            {
                purged += await context.ActionEntries
                    .IgnoreQueryFilters()
                    .Where(e => allEntryIdsToPurge.Contains(e.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // TrackedActionTags
            if (expiredActionIds.Count > 0)
            {
                purged += await context.TrackedActionTags
                    .IgnoreQueryFilters()
                    .Where(t => expiredActionIds.Contains(t.TrackedActionId))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // TrackedActions
            if (expiredActionIds.Count > 0)
            {
                purged += await context.TrackedActions
                    .IgnoreQueryFilters()
                    .Where(a => expiredActionIds.Contains(a.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // FieldDefinitions (no FK children left at this point)
            purged += await context.FieldDefinitions
                .IgnoreQueryFilters()
                .Where(fd => fd.UserId == userId && fd.IsDeleted && fd.DeletedAtUtc <= cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            // Tags
            purged += await context.Tags
                .IgnoreQueryFilters()
                .Where(t => t.UserId == userId && t.IsDeleted && t.DeletedAtUtc <= cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return purged;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

internal static partial class PurgeLogMessages
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Purge of deleted data failed.")]
    public static partial void PurgeFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Purge completed. {Count} rows permanently deleted.")]
    public static partial void PurgeCompleted(this ILogger logger, int count);
}
