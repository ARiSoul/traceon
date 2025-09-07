using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class TagMapper
{
    public static Expression<Func<Entities.Tag, Models.Tag>> Project =>
       t => new Models.Tag
       {
           Id = t.Id,
           Name = t.Name,
           Color = t.Color,
           Description = t.Description
       };

    public static Entities.Tag ToEntity(this Models.Tag model)
    {
        return new Entities.Tag
        {
            Id = model.Id,
            Name = model.Name,
            Color = model.Color,
            Description = model.Description
        };
    }

    public static Models.Tag ToModel(this Entities.Tag entity)
    {
        return new Models.Tag
        {
            Id = entity.Id,
            Name = entity.Name,
            Color = entity.Color,
            Description = entity.Description
        };
    }
}
