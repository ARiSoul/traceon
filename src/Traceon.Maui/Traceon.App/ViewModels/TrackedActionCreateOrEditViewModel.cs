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
    private ITrackedActionRepository _trackedActionRepository;

    [ObservableProperty]
    TrackedAction _trackedAction;

    public TrackedActionCreateOrEditViewModel(ITrackedActionRepository trackedActionRepository)
    {
        Title = "Create or Edit Tracked Action";

        _trackedActionRepository = trackedActionRepository;
    }

    [RelayCommand]
    async Task SaveActionAsync()
    {
        if (TrackedAction == null)
            return;

        if (TrackedAction.Id == Guid.Empty) // new
        {
            TrackedAction.Id = Guid.NewGuid();
            TrackedAction.UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            TrackedAction.CreatedAt = DateTime.UtcNow;
            
            await _trackedActionRepository.AddAsync(TrackedAction);
        }
        else // update
        {
            await _trackedActionRepository.UpdateAsync(TrackedAction);
        }

        await Shell.Current.GoToAsync("..");
    }
}
