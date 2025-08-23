using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class ActionFieldRepository(TraceonDbContext context)
        : BaseRepository<ActionField>(context), IActionFieldRepository
{
    public Task DeleteAsync(Guid actionId, Guid fieldDefinitionId)
    {
        var entity = _dbSet
            .FirstOrDefault(af => af.ActionId == actionId && af.FieldDefinitionId == fieldDefinitionId);

        if (entity != null)
            _dbSet.Remove(entity);

        return Task.CompletedTask;
    }
}
