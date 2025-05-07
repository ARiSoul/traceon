using Arisoul.Traceon.App.ViewModels;

namespace Arisoul.Traceon.App.Views;

public partial class FieldDefinitionsPage : ContentPage
{
    private readonly FieldDefinitionsViewModel _viewModel;

    public FieldDefinitionsPage(FieldDefinitionsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        await _viewModel.LoadFieldDefinitionsAsync();
        base.OnAppearing();
    }

    private void SearchBarTop_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel.SearchCommand.CanExecute(e.NewTextValue))
            _viewModel.SearchCommand.Execute(e.NewTextValue);
    }
}