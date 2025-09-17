using Arisoul.Traceon.App.Messages;
using Arisoul.Traceon.App.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace Arisoul.Traceon.App.Views;

public partial class MainPage : ContentPage
{
    MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        WeakReferenceMessenger.Default.Register<NavigateToTrackedActionsMessage>(this, async (r, m) =>
        {
            var vm = new TrackedActionsViewModel(m.Value.Item1)
            {
                IsSelectionMode = m.Value.Item2,
                ActionsToHide = [.. m.Value.Item3]
            };

            await Navigation.PushAsync(new TrackedActionsPage(vm));
        });
    }
}

