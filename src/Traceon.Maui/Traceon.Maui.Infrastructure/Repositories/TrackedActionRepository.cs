using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Arisoul.Traceon.Maui.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TrackedActionRepository(TraceonDbContext context)
        : BaseRepository<TrackedAction>(context), ITrackedActionRepository
{

    #region Public Methods

    #region Actions Overrides

    public override async Task<IEnumerable<TrackedAction>> GetAllAsync()
    {
        return await _dbSet
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
    }

    public override async Task<TrackedAction?> GetByIdAsync(Guid id)
    {
        return await _dbSet
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
    }

    #endregion Actions Overrides

    #region Entries

    public async Task<IEnumerable<ActionEntry>> GetActionEntriesAsync(Guid actionId)
    {
        var action = await GetByIdAsync(actionId).ConfigureAwait(false);
        return action!.Entries;
    }

    public async Task<ActionEntry?> GetActionEntryAsync(Guid actionId, Guid id)
    {
        var action = await GetByIdAsync(actionId).ConfigureAwait(false);
        return action!.Entries.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddActionEntryAsync(Guid actionId, ActionEntry entry)
    {
        var action = await GetByIdAsync(actionId).ConfigureAwait(false);
        action!.Entries.Add(entry);
        await UpdateAsync(action).ConfigureAwait(false);
    }

    public async Task UpdateActionEntryAsync(Guid actionId, ActionEntry entry)
    {
        var action = await GetByIdAsync(actionId).ConfigureAwait(false);
        var existingEntry = action!.Entries.FirstOrDefault(x => x.Id == entry.Id);

        if (existingEntry is not null)
        {
            existingEntry = entry;
            await UpdateAsync(action).ConfigureAwait(false);
        }
    }

    public async Task DeleteActionEntryAsync(Guid actionId, Guid id)
    {
        var action = await GetByIdAsync(actionId).ConfigureAwait(false);

        if (action is not null)
        {
            action.Entries.RemoveAll(x => x.Id == id);
            await UpdateAsync(action).ConfigureAwait(false);
        }
    }

    #endregion Entries

    #endregion Public Methods
}
