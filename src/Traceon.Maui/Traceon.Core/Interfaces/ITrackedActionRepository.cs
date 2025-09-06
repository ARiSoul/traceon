using Arisoul.Core.Root.Models;
using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface ITrackedActionRepository 
    : IBaseRepository<TrackedAction, Models.TrackedAction>
{
    Task<Result> AddActionEntryAsync(Guid actionId, Models.ActionEntry entry);
    Task<Result> DeleteActionEntryAsync(Guid actionId, Guid id);
    Task<Result<IEnumerable<Models.ActionEntry>>> GetActionEntriesAsync(Guid actionId, bool asNoTracking);
    Task<Result<Models.ActionEntry>> GetActionEntryAsync(Guid actionId, Guid id, bool asNoTracking);
    Task<Result> UpdateActionEntryAsync(Guid actionId, Models.ActionEntry entry);
}
