using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class TrackedActionMapper
{
    public static Expression<Func<Entities.TrackedAction, Models.TrackedAction>> Project =>
        a => new Models.TrackedAction
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            EntryCount = a.Entries.Count,
            Tags = new ObservableCollection<Models.ActionTag>(
                a.Tags.AsQueryable().Select(ActionTagMapper.Project).ToList()),
            Fields = new ObservableCollection<Models.ActionField>(
               a.Fields.AsQueryable().Select(ActionFieldMapper.Project).ToList())
        };

    public static Entities.TrackedAction ToEntity(this Models.TrackedAction model)
    {
        return new Entities.TrackedAction
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Tags = [.. model.Tags.Select(t => t.ToEntity())],
            Fields = [.. model.Fields.Select(f => f.ToEntity())]
        };
    }
}
