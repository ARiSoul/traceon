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

    public async Task<IEnumerable<TrackedAction>> GetAllAsync(Guid userId)
    {
        return await _storage.LoadAsync();
    }

    public async Task<TrackedAction?> GetByIdAsync(Guid id)
    {
        var all = await _storage.LoadAsync();
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddAsync(TrackedAction action)
    {
        var all = await _storage.LoadAsync();
        all.Add(action);
        await _storage.SaveAsync(all);
    }

    public async Task UpdateAsync(TrackedAction action)
    {
        var all = await _storage.LoadAsync();
        var index = all.FindIndex(x => x.Id == action.Id);
        if (index != -1)
        {
            all[index] = action;
            await _storage.SaveAsync(all);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var all = await _storage.LoadAsync();
        all.RemoveAll(x => x.Id == id);
        await _storage.SaveAsync(all);
    }
}
