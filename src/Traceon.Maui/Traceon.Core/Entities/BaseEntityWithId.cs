using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Core.Entities;

public class BaseEntityWithId : IEntityWithId
{
    public Guid Id { get; set; }
}
