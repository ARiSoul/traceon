using Arisoul.Traceon.App.ViewModels;

namespace Arisoul.Traceon.App.Views;

public partial class TrackedActionCreateOrEditPage : ContentPage
{
	TrackedActionCreateOrEditViewModel _viewModel;

	public TrackedActionCreateOrEditPage(TrackedActionCreateOrEditViewModel vm)
	{
		InitializeComponent();
        BindingContext = _viewModel = vm;
    }
}