using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface ITrackedActionRepository 
    : IBaseRepository<TrackedAction>
{
    Task<IEnumerable<TrackedAction>> GetAllAsync(Guid userId);
}
