using Arisoul.Traceon.Maui.Core.Entities;
using Riok.Mapperly.Abstractions;

namespace Arisoul.Traceon.Maui.Core;

[Mapper(UseDeepCloning = true)]
public partial class MapperlyConfiguration
{
    public partial TrackedAction Map(Models.TrackedAction source);
    public partial IEnumerable<TrackedAction> Map(IEnumerable<Models.TrackedAction> source);
    public partial Models.TrackedAction Map(TrackedAction source);
    public partial IEnumerable<Models.TrackedAction> Map(IEnumerable<TrackedAction> source);
    public partial ActionField Map(Models.ActionField source);
    public partial IEnumerable<ActionField> Map(IEnumerable<Models.ActionField> source);
    public partial Models.ActionField Map(ActionField source);
    public partial IEnumerable<Models.ActionField> Map(IEnumerable<ActionField> source);
    public partial ActionTag Map(Models.ActionTag source);
    public partial IEnumerable<ActionTag> Map(IEnumerable<Models.ActionTag> source);
    public partial Models.ActionTag Map(ActionTag source);
    public partial IEnumerable<Models.ActionTag> Map(IEnumerable<ActionTag> source);
    public partial ActionEntry Map(Models.ActionEntry source);
    public partial IEnumerable<ActionEntry> Map(IEnumerable<Models.ActionEntry> source);
    public partial Models.ActionEntry Map(ActionEntry source);
    public partial IEnumerable<Models.ActionEntry> Map(IEnumerable<ActionEntry> source);
    public partial ActionEntryField Map(Models.ActionEntryField source);
    public partial IEnumerable<ActionEntryField> Map(IEnumerable<Models.ActionEntryField> source);
    public partial Models.ActionEntryField Map(ActionEntryField source);
    public partial IEnumerable<Models.ActionEntryField> Map(IEnumerable<ActionEntryField> source);
    public partial FieldDefinition Map(Models.FieldDefinition source);
    public partial IEnumerable<FieldDefinition> Map(IEnumerable<Models.FieldDefinition> source);
    public partial Models.FieldDefinition Map(FieldDefinition source);
    public partial IEnumerable<Models.FieldDefinition> Map(IEnumerable<FieldDefinition> source);
    public partial Tag Map(Models.Tag source);
    public partial IEnumerable<Tag> Map(IEnumerable<Models.Tag> source);
    public partial Models.Tag Map(Tag source);
    public partial IEnumerable<Models.Tag> Map(IEnumerable<Tag> source);
}
