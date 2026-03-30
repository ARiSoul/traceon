using Arisoul.Traceon.App.ViewModels;

namespace Arisoul.Traceon.App.Views;

public partial class FieldDefinitionCreateOrEditPage : ContentPage
{
    readonly FieldDefinitionCreateOrEditViewModel _viewModel;

	public FieldDefinitionCreateOrEditPage(FieldDefinitionCreateOrEditViewModel vm)
	{
		InitializeComponent();
        BindingContext = _viewModel = vm;
    }
}