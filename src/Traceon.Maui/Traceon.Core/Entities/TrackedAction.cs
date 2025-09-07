using System.Collections.ObjectModel;

namespace Arisoul.Traceon.Maui.Core.Entities;

public class TrackedAction
    : BaseEntityWithId
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ObservableCollection<ActionField> Fields { get; set; } = [];
    public ObservableCollection<ActionTag> Tags { get; set; } = [];
    public ObservableCollection<ActionEntry> Entries { get; set; } = [];
}
