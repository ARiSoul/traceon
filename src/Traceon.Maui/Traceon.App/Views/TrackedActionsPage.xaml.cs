using Arisoul.Traceon.App.ViewModels;
using Arisoul.Traceon.Maui.Core.Entities;

namespace Arisoul.Traceon.App.Views;

public partial class TrackedActionsPage : ContentPage
{
	TrackedActionsViewModel _viewModel;

	public TrackedActionsPage(TrackedActionsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = _viewModel = viewModel;

        Task.Run(_viewModel.LoadActionsAsync);
    }

    private void OnActionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TrackedAction action)
        {
            (BindingContext as TrackedActionsViewModel)?.SelectAction(action);
        }
    }
}