using System.Collections.ObjectModel;

namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class ActionEntryMapper
{
    public static System.Linq.Expressions.Expression<Func<Entities.ActionEntry, Models.ActionEntry>> Project =>
           e => new Models.ActionEntry
           {
               Id = e.Id,
               ActionId = e.ActionId,
               Timestamp = e.Timestamp,
               Fields = e.Fields != null ? new ObservableCollection<Models.ActionEntryField>(
                   e.Fields.AsQueryable().Select(ActionEntryFieldMapper.Project)) 
                    : new ObservableCollection<Models.ActionEntryField>()
           };

    public static Entities.ActionEntry ToEntity(this Models.ActionEntry model)
    {
        return new Entities.ActionEntry
        {
            Id = model.Id,
            ActionId = model.ActionId,
            Timestamp = model.Timestamp,
            Fields = [.. model.Fields.Select(t => t.ToEntity())]
        };
    }

    public static Models.ActionEntry ToModel(this Entities.ActionEntry entity)
    {
        return new Models.ActionEntry
        {
            Id = entity.Id,
            ActionId = entity.ActionId,
            Timestamp = entity.Timestamp,
            Fields = entity.Fields != null ? new ObservableCollection<Models.ActionEntryField>(
                entity.Fields.AsQueryable().Select(ActionEntryFieldMapper.Project))
                : []
        };
    }
}
