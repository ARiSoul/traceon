using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Infrastructure.Storage;

public class JsonTrackedActionRepository 
    : ITrackedActionRepository
{
    private readonly FileStorage<TrackedAction> _storage;

    public JsonTrackedActionRepository(string userId)
    {
        var path = Path.Combine("Data", userId, "actions.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _storage = new FileStorage<TrackedAction>(path);
    }

    #region Public Methods

    #region Actions

    public async Task<IEnumerable<TrackedAction>> GetAllAsync(Guid userId)
    {
        return await _storage.LoadAsync().ConfigureAwait(false);
    }

    public async Task<TrackedAction?> GetByIdAsync(Guid id)
    {
        var all = await _storage.LoadAsync().ConfigureAwait(false);
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddAsync(TrackedAction action)
    {
        var all = await _storage.LoadAsync().ConfigureAwait(false);
        all.Add(action);
        await _storage.SaveAsync(all).ConfigureAwait(false);
    }

    public async Task UpdateAsync(TrackedAction action)
    {
        var all = await _storage.LoadAsync().ConfigureAwait(false);
        var index = all.FindIndex(x => x.Id == action.Id);
        if (index != -1)
        {
            all[index] = action;
            await _storage.SaveAsync(all).ConfigureAwait(false);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var all = await _storage.LoadAsync().ConfigureAwait(false);
        all.RemoveAll(x => x.Id == id);
        await _storage.SaveAsync(all).ConfigureAwait(false);
    }

    #endregion Actions

    #region Entries

    public async Task<IEnumerable<ActionEntry>> GetActionEntriesAsync(Guid actionId)
    {
        var action = await this.GetByIdAsync(actionId).ConfigureAwait(false);
        return action!.Entries;
    }

    public async Task<ActionEntry?> GetActionEntryAsync(Guid actionId, Guid id)
    {
        var action = await this.GetByIdAsync(actionId).ConfigureAwait(false);
        return action!.Entries.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddActionEntryAsync(Guid actionId, ActionEntry entry)
    {
        var action = await this.GetByIdAsync(actionId).ConfigureAwait(false);
        action!.Entries.Add(entry);
        await this.UpdateAsync(action).ConfigureAwait(false);
    }

    public async Task UpdateActionEntryAsync(Guid actionId, ActionEntry entry)
    {
        var action = await this.GetByIdAsync(actionId).ConfigureAwait(false);
        var existingEntry = action!.Entries.FirstOrDefault(x => x.Id == entry.Id);

        if (existingEntry is not null)
        {
            existingEntry = entry;
            await this.UpdateAsync(action).ConfigureAwait(false);
        }
    }

    public async Task DeleteActionEntryAsync(Guid actionId, Guid id)
    {
        var action = await this.GetByIdAsync(actionId).ConfigureAwait(false);

        if (action is not null)
        {
            action.Entries.RemoveAll(x => x.Id == id);
            await this.UpdateAsync(action).ConfigureAwait(false);
        }
    }

    #endregion Entries

    #endregion Public Methods
}
