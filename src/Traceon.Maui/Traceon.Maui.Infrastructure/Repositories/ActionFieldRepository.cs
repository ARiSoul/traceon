using Arisoul.Core.Root.Models;
using Arisoul.Core.Root.Models.Results;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Core.Mappings;
using Arisoul.Traceon.Maui.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Infrastructure.Repositories;

public class ActionFieldRepository(TraceonDbContext context)
        : BaseRepository<ActionField, Core.Models.ActionField>(context), IActionFieldRepository
{
    protected override Expression<Func<ActionField, Core.Models.ActionField>> GetProjectExpression()
        => ActionFieldMapper.Project;

    protected override ActionField MapModelToEntity(Core.Models.ActionField model)
        => ActionFieldMapper.ToEntity(model);

    public async Task<Result> DeleteAsync(Guid actionId, Guid fieldDefinitionId)
    {
        var entity = await DbSet
            .FirstOrDefaultAsync(af => af.ActionId == actionId && af.FieldDefinitionId == fieldDefinitionId)
            .ConfigureAwait(false);

        if (entity == null)
            return new ResultNotFoundError($"{typeof(ActionField).Name} with ActionId '{actionId}' and FieldDefinitionId '{fieldDefinitionId}' not found.");

        DbSet.Remove(entity);

        return Result.Success();
    }
}
