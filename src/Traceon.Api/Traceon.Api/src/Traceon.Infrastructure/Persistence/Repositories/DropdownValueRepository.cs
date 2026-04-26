using Microsoft.EntityFrameworkCore;
using Traceon.Domain.Entities;
using Traceon.Domain.Repositories;

namespace Traceon.Infrastructure.Persistence.Repositories;

internal sealed class DropdownValueRepository(TraceonDbContext context) : IDropdownValueRepository
{
    public async Task<DropdownValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.DropdownValues.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdAsync(
        Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        return await context.DropdownValues
            .AsNoTracking()
            .Where(dv => dv.FieldDefinitionId == fieldDefinitionId)
            .OrderBy(dv => dv.SortOrder)
            .ThenBy(dv => dv.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DropdownValue>> GetByFieldDefinitionIdsAsync(
        IEnumerable<Guid> fieldDefinitionIds, CancellationToken cancellationToken = default)
    {
        var ids = fieldDefinitionIds.ToList();
        return await context.DropdownValues
            .AsNoTracking()
            .Where(dv => ids.Contains(dv.FieldDefinitionId))
            .OrderBy(dv => dv.SortOrder)
            .ThenBy(dv => dv.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DropdownValue entity, CancellationToken cancellationToken = default)
    {
        context.DropdownValues.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<DropdownValue> entities, CancellationToken cancellationToken = default)
    {
        context.DropdownValues.AddRange(entities);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DropdownValue entity, CancellationToken cancellationToken = default)
    {
        context.DropdownValues.Update(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await context.DropdownValues
            .Where(dv => dv.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByFieldDefinitionIdAsync(Guid fieldDefinitionId, CancellationToken cancellationToken = default)
    {
        await context.DropdownValues
            .Where(dv => dv.FieldDefinitionId == fieldDefinitionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task CascadeRenameAsync(
        Guid fieldDefinitionId,
        string oldValue,
        string newValue,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Find all ActionField IDs that use this FieldDefinition
            var actionFieldIds = await context.ActionFields
                .AsNoTracking()
                .Where(af => af.FieldDefinitionId == fieldDefinitionId)
                .Select(af => af.Id)
                .ToListAsync(cancellationToken);

            if (actionFieldIds.Count == 0)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            // 2. Cascade to ActionEntryFieldValues (the value list lives in the child table now)
            await context.ActionEntryFieldValues
                .Where(v => v.Value == oldValue
                            && context.ActionEntryFields
                                .Where(aef => actionFieldIds.Contains(aef.ActionFieldId))
                                .Select(aef => aef.Id)
                                .Contains(v.ActionEntryFieldId))
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Value, newValue), cancellationToken);

            // 3. Cascade to FieldDependencyRules — SourceValue
            await context.FieldDependencyRules
                .Where(r => actionFieldIds.Contains(r.SourceFieldId) && r.SourceValue == oldValue)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.SourceValue, newValue), cancellationToken);

            // 4. Cascade to FieldDependencyRules — TargetConstraint (pipe-delimited)
            var targetDepRules = await context.FieldDependencyRules
                .Where(r => actionFieldIds.Contains(r.TargetFieldId)
                         && r.TargetConstraint != null
                         && r.TargetConstraint.Contains(oldValue))
                .ToListAsync(cancellationToken);

            foreach (var rule in targetDepRules)
            {
                var values = rule.TargetConstraint!.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var updated = values.Select(v => v == oldValue ? newValue : v);
                rule.Update(targetConstraint: string.Join('|', updated));
            }

            // 5. Cascade to ReceiptMappingRules
            var receiptConfigIds = await context.ReceiptImportConfigs
                .AsNoTracking()
                .Select(c => new { c.Id, c.TrackedActionId })
                .ToListAsync(cancellationToken);

            // Get TrackedActionIds that have any of our ActionFields
            var trackedActionIds = await context.ActionFields
                .AsNoTracking()
                .Where(af => af.FieldDefinitionId == fieldDefinitionId)
                .Select(af => af.TrackedActionId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var relevantConfigIds = receiptConfigIds
                .Where(c => trackedActionIds.Contains(c.TrackedActionId))
                .Select(c => c.Id)
                .ToList();

            if (relevantConfigIds.Count > 0)
            {
                await context.ReceiptMappingRules
                    .Where(r => relevantConfigIds.Contains(r.ReceiptImportConfigId)
                             && actionFieldIds.Contains(r.TargetFieldId)
                             && r.Value == oldValue)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.Value, newValue), cancellationToken);
            }

            // 6. Cascade to FieldAnalyticsRules — FilterValue
            await context.FieldAnalyticsRules
                .Where(r => trackedActionIds.Contains(r.TrackedActionId)
                         && ((actionFieldIds.Contains(r.MeasureFieldId) && r.FilterValue == oldValue)
                          || (r.FilterFieldId.HasValue && actionFieldIds.Contains(r.FilterFieldId.Value) && r.FilterValue == oldValue)))
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.FilterValue, newValue), cancellationToken);

            // 7. Cascade to FieldAnalyticsRules — NegativeValues (comma-delimited)
            var analyticsWithNegValues = await context.FieldAnalyticsRules
                .Where(r => trackedActionIds.Contains(r.TrackedActionId)
                         && r.NegativeValues != null
                         && r.NegativeValues.Contains(oldValue))
                .ToListAsync(cancellationToken);

            foreach (var rule in analyticsWithNegValues)
            {
                var values = rule.NegativeValues!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var updated = values.Select(v => v == oldValue ? newValue : v);
                rule.Update(
                    filterFieldId: rule.FilterFieldId,
                    filterValue: rule.FilterValue,
                    negativeValues: string.Join(',', updated),
                    signFieldId: rule.SignFieldId);
            }

            // 8. Cascade to ConnectedActionRules — ConditionsJson & FieldMappingsJson
            var connRules = await context.ConnectedActionRules
                .Where(r => (trackedActionIds.Contains(r.SourceTrackedActionId) || trackedActionIds.Contains(r.TargetTrackedActionId))
                         && ((r.ConditionsJson != null && r.ConditionsJson.Contains(oldValue))
                          || (r.FieldMappingsJson != null && r.FieldMappingsJson.Contains(oldValue))))
                .ToListAsync(cancellationToken);

            foreach (var rule in connRules)
            {
                var newConditions = rule.ConditionsJson?.Replace(oldValue, newValue);
                var newMappings = rule.FieldMappingsJson?.Replace(oldValue, newValue);
                rule.Update(
                    conditionsJson: newConditions,
                    fieldMappingsJson: newMappings);
            }

            // 9. Update the FieldDefinition.DropdownValues pipe-delimited string
            var fieldDef = await context.FieldDefinitions.FindAsync([fieldDefinitionId], cancellationToken);
            if (fieldDef is not null && fieldDef.DropdownValues is not null)
            {
                var ddValues = fieldDef.DropdownValues.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var updatedDdValues = ddValues.Select(v => v == oldValue ? newValue : v);
                var newDdString = string.Join('|', updatedDdValues);
                fieldDef.Update(
                    fieldDef.DefaultName,
                    fieldDef.Type,
                    fieldDef.DefaultDescription,
                    newDdString,
                    fieldDef.DefaultMaxValue,
                    fieldDef.DefaultMinValue,
                    fieldDef.DefaultIsRequired,
                    fieldDef.DefaultValue == oldValue ? newValue : fieldDef.DefaultValue,
                    fieldDef.Unit);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}