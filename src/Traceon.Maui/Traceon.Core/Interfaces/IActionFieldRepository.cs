using Arisoul.Core.Root.Models;
using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IActionFieldRepository
    : IBaseRepository<ActionField, Models.ActionField>
{
    Task<Result> DeleteAsync(Guid actionId, Guid fieldDefinitionId);
}