using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Core.Mappings;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class TagRepository(TraceonDbContext context)
        : BaseRepository<Tag, Core.Models.Tag>(context), ITagRepository
{
    protected override Expression<Func<Tag, Core.Models.Tag>> GetProjectExpression() 
        => TagMapper.Project;

    protected override Tag MapModelToEntity(Core.Models.Tag model)
        => TagMapper.ToEntity(model);
}