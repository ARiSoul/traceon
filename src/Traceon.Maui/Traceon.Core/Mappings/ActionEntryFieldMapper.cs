namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class ActionEntryFieldMapper
{
    public static System.Linq.Expressions.Expression<Func<Entities.ActionEntryField, Models.ActionEntryField>> Project =>
           f => new Models.ActionEntryField
           {
               Id = f.Id,
               ActionEntryId = f.ActionEntryId,
               ActionFieldId = f.ActionFieldId,
               Value = f.Value,
               ActionField = f.ActionField != null ? f.ActionField.ToModel() : null!
           };
    
    public static Entities.ActionEntryField ToEntity(this Models.ActionEntryField model)
    {
        return new Entities.ActionEntryField
        {
            Id = model.Id,
            ActionEntryId = model.ActionEntryId,
            ActionFieldId = model.ActionFieldId,
            Value = model.Value,
            FieldDefinitionId = model.FieldDefinitionId
        };
    }
}
