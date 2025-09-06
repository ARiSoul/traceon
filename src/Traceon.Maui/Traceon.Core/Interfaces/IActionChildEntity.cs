using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.Maui.Core.Interfaces;

public interface IActionChildEntity
{
    Guid ActionId { get; set; }
    TrackedAction Action { get; set; }
}
