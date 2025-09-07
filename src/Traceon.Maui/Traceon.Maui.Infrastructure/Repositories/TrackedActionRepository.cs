using Arisoul.Core.Root.Models;
using Arisoul.Core.Root.Models.Results;
using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Core.Mappings;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TrackedActionRepository(TraceonDbContext context)
        : BaseRepository<TrackedAction, Core.Models.TrackedAction>(context), ITrackedActionRepository
{

    #region Public Methods

    #region Actions Overrides

    protected override IQueryable<TrackedAction> IncludeNavigationProperties(DbSet<TrackedAction> dbSet, bool asNoTracking)
    {
        var query = base.IncludeNavigationProperties(dbSet, asNoTracking);

        query = query
            .Include(a => a.Tags)
            .Include(a => a.Fields)
                .ThenInclude(af => af.FieldDefinition);

        return query;
    }

    protected override Expression<Func<TrackedAction, Core.Models.TrackedAction>> GetProjectExpression() 
        => TrackedActionMapper.Project;

    protected override void OnAfterUpdateValuesInEntity(Core.Models.TrackedAction model, TrackedAction updatedEntity)
    {
        // Compare fields and update the collection accordingly
        foreach (var field in model.Fields)
        {
            var existingField = updatedEntity.Fields.FirstOrDefault(f => f.Id == field.Id);
            if (existingField != null)
            {
                // Update existing field
                existingField.Name = field.Name;
                existingField.Description = field.Description;
                existingField.IsRequired = field.IsRequired;
                existingField.MinValue = field.MinValue;
                existingField.MaxValue = field.MaxValue;
                existingField.FieldDefinitionId = field.FieldDefinitionId;
            }
            else
            {
                // Add new field
                updatedEntity.Fields.Add(ActionFieldMapper.ToEntity(field));
            }
        }

        // Remove fields that are no longer present
        var fieldsToRemove = updatedEntity.Fields.Where(f => !model.Fields.Any(mf => mf.Id == f.Id)).ToList();
        foreach (var fieldToRemove in fieldsToRemove)
            updatedEntity.Fields.Remove(fieldToRemove);
    }

    protected override TrackedAction MapModelToEntity(Core.Models.TrackedAction model) 
        => TrackedActionMapper.ToEntity(model);

    #endregion Actions Overrides

    #region Entries

    public async Task<Result<IEnumerable<Core.Models.ActionEntry>>> GetActionEntriesAsync(Guid actionId, bool asNoTracking)
    {
        var query = Context.ActionEntries
            .AsSplitQuery()
            .Where(e => e.ActionId == actionId);

        if (asNoTracking)
            query = query.AsNoTracking();

        var entries = await query
            .Select(ActionEntryMapper.Project)
            .ToListAsync()
            .ConfigureAwait(false);

        return entries;
    }

    public async Task<Result<Core.Models.ActionEntry>> GetActionEntryAsync(Guid actionId, Guid id, bool asNoTracking)
    {
        var query = Context.ActionEntries
            .AsSplitQuery()
            .Where(e => e.ActionId == actionId && e.Id == id);

        if (asNoTracking)
            query = query.AsNoTracking();

        var entry = await query
            .Select(ActionEntryMapper.Project)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (entry is null)
            return new ResultNotFoundError($"ActionEntry with Id '{id}' not found in Action with Id '{actionId}'.");

        return entry;
    }

    public async Task<Result> AddActionEntryAsync(Guid actionId, Core.Models.ActionEntry entry)
    {
        var actionResult = await GetByIdAsync(actionId, false).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        var actionEntryResult = await GetActionEntryAsync(actionId, entry.Id, true).ConfigureAwait(false);

        if (actionEntryResult.Succeeded)
            return new ResultConflictError($"ActionEntry with Id '{entry.Id}' already exists in Action with Id '{actionId}'.");

        this.Context.ActionEntries.Add(ActionEntryMapper.ToEntity(entry));

        return Result.Success();
    }

    public async Task<Result> UpdateActionEntryAsync(Guid actionId, Core.Models.ActionEntry entry)
    {
        var actionResult = await GetByIdAsync(actionId, false).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        var existingEntry = this.Context.ActionEntries
            .AsSplitQuery()
            .Include(e => e.Fields)
            .FirstOrDefault(x => x.Id == entry.Id);

        if (existingEntry is null)
            return new ResultNotFoundError($"ActionEntry with Id '{entry.Id}' not found.");

        var modifiedEntity = ActionEntryMapper.ToEntity(entry);

        this.Context.Entry(existingEntry).CurrentValues.SetValues(modifiedEntity);

        // Handle Fields changes
        foreach (var field in entry.Fields)
        {
            var existingField = modifiedEntity.Fields.FirstOrDefault(f => f.Id == field.Id);
            if (existingField != null)
            {
                // Update existing field
                existingField.Value = field.Value;
                existingField.ActionEntryId = field.ActionEntryId;
                existingField.ActionFieldId = field.ActionFieldId;
                existingField.FieldDefinitionId = field.FieldDefinitionId;
            }
            else
            {
                // Add new field
                modifiedEntity.Fields.Add(ActionEntryFieldMapper.ToEntity(field));
            }
        }

        // Remove fields that are no longer present
        var fieldsToRemove = modifiedEntity.Fields.Where(f => !entry.Fields.Any(mf => mf.Id == f.Id)).ToList();
        foreach (var fieldToRemove in fieldsToRemove)
            modifiedEntity.Fields.Remove(fieldToRemove);

        return Result.Success();
    }

    public async Task<Result> DeleteActionEntryAsync(Guid actionId, Guid id)
    {
        var actionResult = await GetByIdAsync(actionId, false).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        var existingEntry = this.Context.ActionEntries
            .AsSplitQuery()
            .Include(e => e.Fields)
            .FirstOrDefault(x => x.Id == id);

        if (existingEntry is null)
            return new ResultNotFoundError($"ActionEntry with Id '{id}' not found.");

        this.Context.ActionEntries.Remove(existingEntry);

        return Result.Success();
    }

    #endregion Entries

    #endregion Public Methods
}
