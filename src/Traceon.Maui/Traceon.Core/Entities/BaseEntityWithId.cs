using Arisoul.Core.Root.Models.Base;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arisoul.Traceon.Maui.Core.Entities;

public partial class BaseEntityWithId 
    : BaseObservableValidator, IEntityWithId
{
    [ObservableProperty] Guid _id;
}
