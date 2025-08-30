using Arisoul.Core.Root.Models;
using Arisoul.Core.Root.Models.Results;
using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TrackedActionRepository(TraceonDbContext context, MapperlyConfiguration mapper)
        : BaseRepository<TrackedAction, Core.Models.TrackedAction>(context, mapper), ITrackedActionRepository
{

    #region Public Methods

    #region Actions Overrides

    public override IEnumerable<Core.Models.TrackedAction> MapEntityToModelCollection(IEnumerable<TrackedAction> entities)
        => Mapper.Map(entities);

    public override Core.Models.TrackedAction MapEntityToModel(TrackedAction entity)
        => Mapper.Map(entity);

    public override TrackedAction MapModelToEntity(Core.Models.TrackedAction model)
        => Mapper.Map(model);

    public override async Task<Result<IEnumerable<Core.Models.TrackedAction>>> GetAllAsync()
    {
        var entities = await this.DbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Tags)
            .Include(a => a.Entries)
            .ThenInclude(e => e.Fields)
                .ThenInclude(ef => ef.FieldDefinition)
            .Include(a => a.Fields)
                .ThenInclude(af => af.FieldDefinition)
                .ToListAsync()
                .ConfigureAwait(false);

        return Result.Success(MapEntityToModelCollection(entities));
    }

    public override async Task<Result<Core.Models.TrackedAction>> GetByIdAsync(Guid id)
    {
        var entity = await DbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Tags)
            .Include(a => a.Entries)
            .ThenInclude(e => e.Fields)
                .ThenInclude(ef => ef.FieldDefinition)
            .Include(a => a.Fields)
                .ThenInclude(af => af.FieldDefinition)
            .FirstOrDefaultAsync(a => a.Id == id)
            .ConfigureAwait(false);

        if (entity == null)
            return new ResultNotFoundError($"{typeof(TrackedAction).Name} with Id '{id}' not found.");

        return MapEntityToModel(entity);
    }

    #endregion Actions Overrides

    #region Entries

    public async Task<Result<IEnumerable<Core.Models.ActionEntry>>> GetActionEntriesAsync(Guid actionId)
    {
        var actionResult = await GetByIdAsync(actionId).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        return actionResult.Value.Entries;
    }

    public async Task<Result<Core.Models.ActionEntry>> GetActionEntryAsync(Guid actionId, Guid id)
    {
        var actionResult = await GetByIdAsync(actionId).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        var entry = actionResult.Value.Entries.FirstOrDefault(x => x.Id == id);

        if (entry is null)
            return new ResultNotFoundError($"ActionEntry with Id '{id}' not found.");

        return entry;
    }

    public async Task<Result> AddActionEntryAsync(Guid actionId, Core.Models.ActionEntry entry)
    {
        var actionResult = await GetByIdAsync(actionId).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        actionResult.Value.Entries.Add(entry);

        var updateResult = await UpdateAsync(actionResult.Value).ConfigureAwait(false);

        if (updateResult.Failed)
            return updateResult.Error!;

        return Result.Success();
    }

    public async Task<Result> UpdateActionEntryAsync(Guid actionId, Core.Models.ActionEntry entry)
    {
        var actionResult = await GetByIdAsync(actionId).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        var existingEntry = actionResult.Value.Entries.FirstOrDefault(x => x.Id == entry.Id);

        if (existingEntry is null)
            return new ResultNotFoundError($"ActionEntry with Id '{entry.Id}' not found.");

        existingEntry = entry;
        var updateResult = await UpdateAsync(actionResult.Value).ConfigureAwait(false);

        if (updateResult.Failed)
            return updateResult.Error!;

        return Result.Success();
    }

    public async Task<Result> DeleteActionEntryAsync(Guid actionId, Guid id)
    {
        var actionResult = await GetByIdAsync(actionId).ConfigureAwait(false);

        if (actionResult.Failed)
            return actionResult.Error!;

        actionResult.Value.Entries.RemoveAll(x => x.Id == id);
        var updateResult = await UpdateAsync(actionResult.Value).ConfigureAwait(false);

        if (updateResult.Failed)
            return updateResult.Error!;

        return Result.Success();
    }

    #endregion Entries

    #endregion Public Methods
}
