using Arisoul.Traceon.App.ViewModels;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Localization;
using CommunityToolkit.Maui.Views;

namespace Arisoul.Traceon.App.Views;

public partial class TrackedActionsPage : ContentPage
{
	TrackedActionsViewModel _viewModel;

	public TrackedActionsPage(TrackedActionsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    private void OnActionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TrackedAction action)
        {
            (BindingContext as TrackedActionsViewModel)?.SelectAction(action);
        }
    }

    protected override async void OnAppearing()
    {
        await _viewModel.LoadActionsAsync().ConfigureAwait(false);
        base.OnAppearing();
    }
}