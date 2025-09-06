using Arisoul.Core.Root.Models.Base;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arisoul.Traceon.Maui.Core.Entities;

public partial class BaseActionChildEntity
    : BaseObservableValidator, IActionChildEntity
{
    [ObservableProperty] Guid _actionId;

    public TrackedAction Action { get; set; } = null!;
}
