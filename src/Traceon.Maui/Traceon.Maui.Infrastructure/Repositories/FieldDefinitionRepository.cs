using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Infrastructure.Data;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class FieldDefinitionRepository
    : BaseRepository<FieldDefinition>, IFieldDefinitionRepository
{
    public FieldDefinitionRepository(TraceonDbContext context) : base(context)
    {
    }
}
