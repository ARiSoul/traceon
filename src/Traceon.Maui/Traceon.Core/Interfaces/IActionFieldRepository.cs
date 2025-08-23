using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IActionFieldRepository
    : IBaseRepository<ActionField>
{
    Task DeleteAsync(Guid actionId, Guid fieldDefinitionId);
}