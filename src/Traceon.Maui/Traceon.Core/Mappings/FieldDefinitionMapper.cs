using System.Linq.Expressions;

namespace Arisoul.Traceon.Maui.Core.Mappings;

public static class FieldDefinitionMapper
{
    public static Expression<Func<Entities.FieldDefinition, Models.FieldDefinition>> Project =>
     f => new Models.FieldDefinition
     {
         Id = f.Id,
         DefaultName = f.DefaultName,
         DefaultDescription = f.DefaultDescription,
         DefaultIsRequired = f.DefaultIsRequired,
         DefaultMinValue = f.DefaultMinValue,
         DefaultMaxValue = f.DefaultMaxValue,
         Type = f.Type,
         DropdownValues = f.DropdownValues
     };

    public static Entities.FieldDefinition ToEntity(this Models.FieldDefinition model)
    {
        return new Entities.FieldDefinition
        {
            Id = model.Id,
            DefaultName = model.DefaultName,
            DefaultDescription = model.DefaultDescription,
            DefaultIsRequired = model.DefaultIsRequired,
            DefaultMinValue = model.DefaultMinValue,
            DefaultMaxValue = model.DefaultMaxValue,
            Type = model.Type,
            DropdownValues = model.DropdownValues
        };
    }

    public static Models.FieldDefinition ToModel(this Entities.FieldDefinition entity)
    {
        return new Models.FieldDefinition
        {
            Id = entity.Id,
            DefaultName = entity.DefaultName,
            DefaultDescription = entity.DefaultDescription,
            DefaultIsRequired = entity.DefaultIsRequired,
            DefaultMinValue = entity.DefaultMinValue,
            DefaultMaxValue = entity.DefaultMaxValue,
            Type = entity.Type,
            DropdownValues = entity.DropdownValues
        };
    }
}
