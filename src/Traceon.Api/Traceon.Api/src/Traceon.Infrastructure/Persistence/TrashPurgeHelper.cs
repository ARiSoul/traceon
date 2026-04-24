using Microsoft.EntityFrameworkCore;

namespace Traceon.Infrastructure.Persistence;

internal static class TrashPurgeHelper
{
    /// <summary>
    /// Hard-deletes the given soft-deleted entity sets in FK-safe order, clearing every
    /// Restrict/NoAction dependent before touching the parent row. <paramref name="candidateFieldDefIds"/>
    /// is filtered to those with no remaining <see cref="Traceon.Domain.Entities.ActionField"/> references
    /// outside the current purge scope; the rest are deferred.
    /// </summary>
    public static async Task<int> ExecutePurgeAsync(
        TraceonDbContext context,
        string userId,
        IReadOnlyCollection<Guid> orphanActionIds,
        IReadOnlyCollection<Guid> orphanFieldIds,
        IReadOnlyCollection<Guid> orphanEntryIds,
        IReadOnlyCollection<Guid> candidateFieldDefIds,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var purged = 0;

        var allFieldIdsToPurge = await context.ActionFields
            .IgnoreQueryFilters()
            .Where(af => orphanFieldIds.Contains(af.Id) || orphanActionIds.Contains(af.TrackedActionId))
            .Select(af => af.Id)
            .ToListAsync(cancellationToken);

        var allEntryIdsToPurge = await context.ActionEntries
            .IgnoreQueryFilters()
            .Where(e => orphanEntryIds.Contains(e.Id) || orphanActionIds.Contains(e.TrackedActionId))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        // Defer FieldDefinition purge when any ActionField outside our purge set still references it.
        var blockedFieldDefIds = candidateFieldDefIds.Count == 0
            ? new List<Guid>()
            : await context.ActionFields
                .IgnoreQueryFilters()
                .Where(af => candidateFieldDefIds.Contains(af.FieldDefinitionId)
                    && !allFieldIdsToPurge.Contains(af.Id))
                .Select(af => af.FieldDefinitionId)
                .Distinct()
                .ToListAsync(cancellationToken);

        var fieldDefIdsToPurge = candidateFieldDefIds
            .Where(id => !blockedFieldDefIds.Contains(id))
            .ToList();

        var hasFields = allFieldIdsToPurge.Count > 0;
        var hasEntries = allEntryIdsToPurge.Count > 0;
        var hasActions = orphanActionIds.Count > 0;

        if (hasFields || hasEntries)
        {
            purged += await context.ActionEntryFields
                .IgnoreQueryFilters()
                .Where(ef => allFieldIdsToPurge.Contains(ef.ActionFieldId) || allEntryIdsToPurge.Contains(ef.ActionEntryId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasFields || hasActions)
        {
            purged += await context.FieldAnalyticsRules
                .IgnoreQueryFilters()
                .Where(r => orphanActionIds.Contains(r.TrackedActionId)
                    || allFieldIdsToPurge.Contains(r.MeasureFieldId)
                    || allFieldIdsToPurge.Contains(r.GroupByFieldId)
                    || (r.FilterFieldId != null && allFieldIdsToPurge.Contains(r.FilterFieldId.Value)))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasFields || hasActions)
        {
            purged += await context.FieldDependencyRules
                .IgnoreQueryFilters()
                .Where(r => orphanActionIds.Contains(r.TrackedActionId)
                    || allFieldIdsToPurge.Contains(r.SourceFieldId)
                    || allFieldIdsToPurge.Contains(r.TargetFieldId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasFields || hasActions)
        {
            purged += await context.CustomCharts
                .IgnoreQueryFilters()
                .Where(c => orphanActionIds.Contains(c.TrackedActionId)
                    || allFieldIdsToPurge.Contains(c.MeasureFieldId)
                    || (c.GroupByFieldId != null && allFieldIdsToPurge.Contains(c.GroupByFieldId.Value)))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasFields)
        {
            purged += await context.ReceiptMappingRules
                .IgnoreQueryFilters()
                .Where(r => allFieldIdsToPurge.Contains(r.TargetFieldId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasFields || hasActions)
        {
            purged += await context.ReceiptImportConfigs
                .IgnoreQueryFilters()
                .Where(c => orphanActionIds.Contains(c.TrackedActionId)
                    || (c.ShopFieldId != null && allFieldIdsToPurge.Contains(c.ShopFieldId.Value))
                    || (c.DescriptionFieldId != null && allFieldIdsToPurge.Contains(c.DescriptionFieldId.Value))
                    || (c.TotalFieldId != null && allFieldIdsToPurge.Contains(c.TotalFieldId.Value))
                    || (c.QuantityFieldId != null && allFieldIdsToPurge.Contains(c.QuantityFieldId.Value))
                    || (c.UnitPriceFieldId != null && allFieldIdsToPurge.Contains(c.UnitPriceFieldId.Value))
                    || (c.DiscountFieldId != null && allFieldIdsToPurge.Contains(c.DiscountFieldId.Value))
                    || (c.ReceiptDiscountTypeFieldId != null && allFieldIdsToPurge.Contains(c.ReceiptDiscountTypeFieldId.Value)))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasActions)
        {
            purged += await context.ConnectedActionRules
                .IgnoreQueryFilters()
                .Where(r => orphanActionIds.Contains(r.TargetTrackedActionId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasFields)
        {
            purged += await context.ActionFields
                .IgnoreQueryFilters()
                .Where(af => allFieldIdsToPurge.Contains(af.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasEntries)
        {
            purged += await context.ActionEntries
                .IgnoreQueryFilters()
                .Where(e => allEntryIdsToPurge.Contains(e.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (hasActions)
        {
            purged += await context.TrackedActionTags
                .IgnoreQueryFilters()
                .Where(t => orphanActionIds.Contains(t.TrackedActionId))
                .ExecuteDeleteAsync(cancellationToken);

            purged += await context.TrackedActions
                .IgnoreQueryFilters()
                .Where(a => orphanActionIds.Contains(a.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (fieldDefIdsToPurge.Count > 0)
        {
            purged += await context.FieldDefinitions
                .IgnoreQueryFilters()
                .Where(fd => fieldDefIdsToPurge.Contains(fd.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (tagIds.Count > 0)
        {
            purged += await context.Tags
                .IgnoreQueryFilters()
                .Where(t => t.UserId == userId && tagIds.Contains(t.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        return purged;
    }

    public static int DeferredFieldDefCount(int candidateCount, int purgedCount) => candidateCount - purgedCount;
}
