using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TagRepository(TraceonDbContext context, MapperlyConfiguration mapper)
        : BaseRepository<Tag, Core.Models.Tag>(context, mapper), ITagRepository
{
    public override IEnumerable<Core.Models.Tag> MapEntityToModelCollection(IEnumerable<Tag> entities)
       => Mapper.MapToModelCollection(entities);

    public override Core.Models.Tag MapEntityToModel(Tag entity)
        => Mapper.MapToModel(entity);

    public override Tag MapModelToEntity(Core.Models.Tag model)
        => Mapper.MapToEntity(model);
}