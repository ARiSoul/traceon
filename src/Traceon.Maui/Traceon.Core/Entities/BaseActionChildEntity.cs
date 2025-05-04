using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Core.Entities;

public class BaseActionChildEntity
    : IActionChildEntity
{
    public Guid ActionId { get; set; }
    public TrackedAction Action { get; set; } = null!;
}
