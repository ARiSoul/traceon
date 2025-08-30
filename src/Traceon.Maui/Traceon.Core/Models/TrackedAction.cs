using System.Collections.ObjectModel;

namespace Arisoul.Traceon.Maui.Core.Models;

public class TrackedAction
    : Entities.BaseEntityWithId
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ObservableCollection<ActionField> Fields { get; set; } = [];
    public List<ActionTag> Tags { get; set; } = [];
    public List<ActionEntry> Entries { get; set; } = [];
}
