using Arisoul.Traceon.Maui.Core.Entities;
using Riok.Mapperly.Abstractions;

namespace Arisoul.Traceon.Maui.Core;

[Mapper(UseDeepCloning = true)]
public partial class MapperlyConfiguration
{
    // TrackedAction
    public partial TrackedAction MapToEntity(Models.TrackedAction source);
    public partial IEnumerable<TrackedAction> MapToEntityCollection(IEnumerable<Models.TrackedAction> source);
    public partial Models.TrackedAction MapToModel(TrackedAction source);
    public partial IEnumerable<Models.TrackedAction> MapToModelCollection(IEnumerable<TrackedAction> source);

    // ActionField
    [MapperIgnoreSource(nameof(ActionField.Action))]
    [MapperIgnoreTarget(nameof(ActionField.Action))]
    [MapperIgnoreSource(nameof(ActionField.FieldDefinition))]
    [MapperIgnoreTarget(nameof(ActionField.FieldDefinition))]
    public partial ActionField MapToEntity(Models.ActionField source);
    public partial IEnumerable<ActionField> MapToEntityCollection(IEnumerable<Models.ActionField> source);

    [MapperIgnoreSource(nameof(ActionField.Action))]
    [MapperIgnoreTarget(nameof(ActionField.Action))]
    [MapperIgnoreSource(nameof(ActionField.FieldDefinition))]
    [MapperIgnoreTarget(nameof(ActionField.FieldDefinition))]
    public partial Models.ActionField MapToModel(ActionField source);
    public partial IEnumerable<Models.ActionField> MapToModelCollection(IEnumerable<ActionField> source);

    // ActionTag
    [MapperIgnoreSource(nameof(ActionTag.Action))]
    [MapperIgnoreTarget(nameof(ActionTag.Action))]
    public partial ActionTag MapToEntity(Models.ActionTag source);
    public partial IEnumerable<ActionTag> MapToEntityCollection(IEnumerable<Models.ActionTag> source);

    [MapperIgnoreSource(nameof(ActionTag.Action))]
    [MapperIgnoreTarget(nameof(ActionTag.Action))]
    public partial Models.ActionTag MapToModel(ActionTag source);
    public partial IEnumerable<Models.ActionTag> MapToModelCollection(IEnumerable<ActionTag> source);

    // ActionEntry
    [MapperIgnoreSource(nameof(ActionEntry.Action))]
    [MapperIgnoreTarget(nameof(ActionEntry.Action))]
    public partial ActionEntry MapToEntity(Models.ActionEntry source);
    public partial IEnumerable<ActionEntry> MapToEntityCollection(IEnumerable<Models.ActionEntry> source);

    [MapperIgnoreSource(nameof(ActionEntry.Action))]
    [MapperIgnoreTarget(nameof(ActionEntry.Action))]
    public partial Models.ActionEntry MapToModel(ActionEntry source);
    public partial IEnumerable<Models.ActionEntry> MapToModelCollection(IEnumerable<ActionEntry> source);

    // ActionEntryField
    [MapperIgnoreSource(nameof(ActionEntryField.ActionEntry))]
    [MapperIgnoreTarget(nameof(ActionEntryField.ActionEntry))]
    public partial ActionEntryField MapToEntity(Models.ActionEntryField source);
    public partial IEnumerable<ActionEntryField> MapToEntityCollection(IEnumerable<Models.ActionEntryField> source);

    [MapperIgnoreSource(nameof(ActionEntryField.ActionEntry))]
    [MapperIgnoreTarget(nameof(ActionEntryField.ActionEntry))]
    public partial Models.ActionEntryField MapToModel(ActionEntryField source);
    public partial IEnumerable<Models.ActionEntryField> MapToModelCollection(IEnumerable<ActionEntryField> source);

    // FieldDefinition
    public partial FieldDefinition MapToEntity(Models.FieldDefinition source);
    public partial IEnumerable<FieldDefinition> MapToEntityCollection(IEnumerable<Models.FieldDefinition> source);
    public partial Models.FieldDefinition MapToModel(FieldDefinition source);
    public partial IEnumerable<Models.FieldDefinition> MapToModelCollection(IEnumerable<FieldDefinition> source);

    // Tag
    public partial Tag MapToEntity(Models.Tag source);
    public partial IEnumerable<Tag> MapToEntityCollection(IEnumerable<Models.Tag> source);
    public partial Models.Tag MapToModel(Tag source);
    public partial IEnumerable<Models.Tag> MapToModelCollection(IEnumerable<Tag> source);
}
