using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class FieldDefinitionRepository(TraceonDbContext context, MapperlyConfiguration mapper)
        : BaseRepository<FieldDefinition, Core.Models.FieldDefinition>(context, mapper), IFieldDefinitionRepository
{
    public override IEnumerable<Core.Models.FieldDefinition> MapEntityToModelCollection(IEnumerable<FieldDefinition> entities)
       => Mapper.MapToModelCollection(entities);

    public override Core.Models.FieldDefinition MapEntityToModel(FieldDefinition entity)
        => Mapper.MapToModel(entity);

    public override FieldDefinition MapModelToEntity(Core.Models.FieldDefinition model)
        => Mapper.MapToEntity(model);
}
