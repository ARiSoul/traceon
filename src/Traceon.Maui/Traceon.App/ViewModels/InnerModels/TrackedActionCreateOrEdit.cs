using Arisoul.Core.Root.Models.Base;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arisoul.Traceon.App.ViewModels.InnerModels;

public partial class TrackedActionCreateOrEdit
    : BaseObservableValidator
{
    [ObservableProperty] bool _nameHasError;
}
