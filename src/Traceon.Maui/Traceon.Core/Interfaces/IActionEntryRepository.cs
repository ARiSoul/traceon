using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IActionEntryRepository 
    : IBaseRepository<ActionEntry>
{
    Task<IEnumerable<ActionEntry>> GetAllByActionIdAsync(Guid trackedActionId);
}
