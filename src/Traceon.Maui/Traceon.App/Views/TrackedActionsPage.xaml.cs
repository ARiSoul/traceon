using Arisoul.Traceon.App.ViewModels;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Localization;
using CommunityToolkit.Maui.Views;

namespace Arisoul.Traceon.App.Views;

public partial class TrackedActionsPage : ContentPage
{
    private readonly TrackedActionsViewModel _viewModel;

	public TrackedActionsPage(TrackedActionsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        await _viewModel.LoadActionsAsync().ConfigureAwait(false);
        base.OnAppearing();
    }

    private void SearchBarTop_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel.SearchCommand.CanExecute(e.NewTextValue))
            _viewModel.SearchCommand.Execute(e.NewTextValue);
    }
}