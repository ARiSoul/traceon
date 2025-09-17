using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.App.Messages;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Arisoul.Traceon.App.ViewModels;

public partial class MainPageViewModel
    : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    public MainPageViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        WeakReferenceMessenger.Default.Register<TrackedActionSelectedMessage>(this, (r, m) =>
        {
            var selected = m.Value;
            // TODO: Go to action entry creation page with selected action
        });
    }

    [RelayCommand]
    private void NewActionEntry()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToTrackedActionsMessage(_unitOfWork, true, []));
    }
}
