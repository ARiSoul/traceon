using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Core.Mappings;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class FieldDefinitionRepository(TraceonDbContext context)
        : BaseRepository<FieldDefinition, Core.Models.FieldDefinition>(context), IFieldDefinitionRepository
{
    protected override Expression<Func<FieldDefinition, Core.Models.FieldDefinition>> GetProjectExpression() 
        => FieldDefinitionMapper.Project;

    protected override FieldDefinition MapModelToEntity(Core.Models.FieldDefinition model)
        => FieldDefinitionMapper.ToEntity(model);
}
