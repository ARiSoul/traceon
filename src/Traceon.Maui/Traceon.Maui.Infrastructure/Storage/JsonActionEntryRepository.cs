using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Infrastructure.Storage;

public class JsonActionEntryRepository 
    : IActionEntryRepository
{
    private readonly FileStorage<ActionEntry> _storage;

    public JsonActionEntryRepository(string userId)
    {
        var path = Path.Combine("Data", userId, "entries.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _storage = new FileStorage<ActionEntry>(path);
    }

    public async Task<IEnumerable<ActionEntry>> GetAllByActionIdAsync(Guid trackedActionId)
    {
        var all = await _storage.LoadAsync();
        return all.Where(e => e.TrackedActionId == trackedActionId);
    }

    public async Task<ActionEntry?> GetByIdAsync(Guid id)
    {
        var all = await _storage.LoadAsync();
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddAsync(ActionEntry entry)
    {
        var all = await _storage.LoadAsync();
        all.Add(entry);
        await _storage.SaveAsync(all);
    }

    public async Task UpdateAsync(ActionEntry entry)
    {
        var all = await _storage.LoadAsync();
        var index = all.FindIndex(x => x.Id == entry.Id);
        if (index != -1)
        {
            all[index] = entry;
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
