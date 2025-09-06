using Arisoul.Traceon.Maui.Core;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TagRepository(TraceonDbContext context, MapperlyConfiguration mapper)
        : BaseRepository<Tag, Core.Models.Tag>(context, mapper), ITagRepository
{
    protected override IEnumerable<Core.Models.Tag> MapEntityToModelCollection(IEnumerable<Tag> entities)
       => Mapper.MapToModelCollection(entities);

    protected override Core.Models.Tag MapEntityToModel(Tag entity)
        => Mapper.MapToModel(entity);

    protected override Tag MapModelToEntity(Core.Models.Tag model)
        => Mapper.MapToEntity(model);
}