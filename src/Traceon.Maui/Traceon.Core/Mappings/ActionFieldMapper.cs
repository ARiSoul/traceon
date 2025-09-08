using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class ActionFieldMapper
{
    public static Expression<Func<Entities.ActionField, Models.ActionField>> Project =>
       f => new Models.ActionField
       {
           Id = f.Id,
           ActionId = f.ActionId,
           Name = f.Name,
           Description = f.Description,
           IsRequired = f.IsRequired,
           MinValue = f.MinValue,
           MaxValue = f.MaxValue,
           FieldDefinitionId = f.FieldDefinitionId,
           FieldDefinition = f.FieldDefinition != null ? f.FieldDefinition.ToModel() : null!,
           DefaultValue = f.DefaultValue
       };

    public static Entities.ActionField ToEntity(this Models.ActionField model)
    {
        return new Entities.ActionField
        {
            Id = model.Id,
            ActionId = model.ActionId,
            Name = model.Name,
            Description = model.Description,
            IsRequired = model.IsRequired,
            MinValue = model.MinValue,
            MaxValue = model.MaxValue,
            FieldDefinitionId = model.FieldDefinitionId,
            DefaultValue = model.DefaultValue
        };
    }

    public static Models.ActionField ToModel(this Entities.ActionField entity)
    {
        return new Models.ActionField
        {
            Id = entity.Id,
            ActionId = entity.ActionId,
            Name = entity.Name,
            Description = entity.Description,
            IsRequired = entity.IsRequired,
            MinValue = entity.MinValue,
            MaxValue = entity.MaxValue,
            FieldDefinitionId = entity.FieldDefinitionId,
            FieldDefinition = entity.FieldDefinition != null ? entity.FieldDefinition.ToModel() : null!,
            DefaultValue = entity.DefaultValue
        };
    }
}
