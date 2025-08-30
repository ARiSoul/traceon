using Arisoul.Core.Root.Models.Base;
using Arisoul.Traceon.Maui.Core.Interfaces;

namespace Arisoul.Traceon.Maui.Core.Entities;

public partial class BaseEntityWithId 
    : BaseObservableValidator, IEntityWithId
{
    public Guid Id { get; set; }
}
