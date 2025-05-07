using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arisoul.Traceon.App.ViewModels;

[QueryProperty(nameof(TrackedAction), nameof(TrackedAction))]
public partial class TrackedActionCreateOrEditViewModel 
    : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    TrackedAction _trackedAction;

    public TrackedActionCreateOrEditViewModel(IUnitOfWork unitOfWork)
    {
        Title = "Create or Edit Tracked Action";

        _unitOfWork = unitOfWork;
    }

    [RelayCommand]
    async Task SaveActionAsync()
    {
        if (TrackedAction == null)
            return;

        if (TrackedAction.Id == Guid.Empty) // new
        {
            TrackedAction.Id = Guid.NewGuid();
            
            await _unitOfWork.TrackedActions.CreateAsync(TrackedAction).ConfigureAwait(false);
        }
        else // update
        {
            await _unitOfWork.TrackedActions.UpdateAsync(TrackedAction).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await Shell.Current.GoToAsync("..");
    }
}
