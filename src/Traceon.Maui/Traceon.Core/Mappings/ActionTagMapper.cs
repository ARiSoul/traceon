using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class ActionTagMapper
{
    public static Expression<Func<Entities.ActionTag, Models.ActionTag>> Project =>
        t => new Models.ActionTag
        {
            TagId = t.TagId,
            Tag = t.Tag != null ? t.Tag.ToModel(): null!
        };

    public static Entities.ActionTag ToEntity(this Models.ActionTag model)
    {
        return new Entities.ActionTag
        {
            ActionId = model.ActionId,
            TagId = model.TagId
        };
    }
}
