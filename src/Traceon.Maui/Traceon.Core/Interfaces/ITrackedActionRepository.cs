using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface ITrackedActionRepository 
    : IBaseRepository<TrackedAction>
{
    Task AddActionEntryAsync(Guid actionId, ActionEntry entry);
    Task DeleteActionEntryAsync(Guid actionId, Guid id);
    Task<IEnumerable<ActionEntry>> GetActionEntriesAsync(Guid actionId);
    Task<ActionEntry?> GetActionEntryAsync(Guid actionId, Guid id);
    Task UpdateActionEntryAsync(Guid actionId, ActionEntry entry);
}
