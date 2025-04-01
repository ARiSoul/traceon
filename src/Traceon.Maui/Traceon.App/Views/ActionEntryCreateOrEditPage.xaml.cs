using Arisoul.Traceon.App.ViewModels;

namespace Arisoul.Traceon.App.Views;

public partial class ActionEntryCreateOrEditPage : ContentPage
{
	ActionEntryCreateOrEditViewModel _viewModel;

	public ActionEntryCreateOrEditPage(ActionEntryCreateOrEditViewModel vm)
	{
		InitializeComponent();
        BindingContext = _viewModel = vm;
    }
}