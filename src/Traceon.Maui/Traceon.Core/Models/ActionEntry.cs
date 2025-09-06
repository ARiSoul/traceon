using System.Collections.ObjectModel;

namespace Arisoul.Traceon.Maui.Core.Models;

public class ActionEntry 
    : Entities.BaseEntityWithId
{
    public Guid ActionId { get; set; }
    public TrackedAction Action { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public ObservableCollection<ActionEntryField> Fields { get; set; } = [];
}