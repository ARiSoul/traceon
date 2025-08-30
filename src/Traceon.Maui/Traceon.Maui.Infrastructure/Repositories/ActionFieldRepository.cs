using Arisoul.Core.Root.Models;
using Arisoul.Core.Root.Models.Results;
using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class ActionFieldRepository(TraceonDbContext context, MapperlyConfiguration mapper)
        : BaseRepository<ActionField, Core.Models.ActionField>(context, mapper), IActionFieldRepository
{
    public override IEnumerable<Core.Models.ActionField> MapEntityToModelCollection(IEnumerable<ActionField> entities)
      => Mapper.MapToModelCollection(entities);

    public override Core.Models.ActionField MapEntityToModel(ActionField entity)
        => Mapper.MapToModel(entity);

    public override ActionField MapModelToEntity(Core.Models.ActionField model)
        => Mapper.MapToEntity(model);

    public Task<Result> DeleteAsync(Guid actionId, Guid fieldDefinitionId)
    {
        var entity = DbSet
            .FirstOrDefault(af => af.ActionId == actionId && af.FieldDefinitionId == fieldDefinitionId);

        if (entity == null)
            return Task.FromResult(Result.Failure(new ResultNotFoundError($"{typeof(ActionField).Name} with ActionId '{actionId}' and FieldDefinitionId '{fieldDefinitionId}' not found.")));

        DbSet.Remove(entity);

        return Task.FromResult(Result.Success());
    }
}
