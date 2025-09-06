using Arisoul.Traceon.App.Messages;
using Arisoul.Traceon.App.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace Arisoul.Traceon.App.Views;

public partial class TrackedActionCreateOrEditPage : ContentPage
{
	TrackedActionCreateOrEditViewModel _viewModel;

	public TrackedActionCreateOrEditPage(TrackedActionCreateOrEditViewModel vm)
	{
		InitializeComponent();
        BindingContext = _viewModel = vm;

        WeakReferenceMessenger.Default.Register<NavigateToFieldDefinitionsMessage>(this, async (r, m) =>
        {
            var vm = new FieldDefinitionsViewModel(m.Value.Item1)
            {
                IsSelectionMode = m.Value.Item2,
                FieldsToHide = [.. m.Value.Item3]
            };

            await Navigation.PushAsync(new FieldDefinitionsPage(vm));
        });
    }
}