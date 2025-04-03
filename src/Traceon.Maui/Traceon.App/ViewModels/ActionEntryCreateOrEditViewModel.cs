using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace Arisoul.Traceon.App.ViewModels;

[QueryProperty(nameof(TrackedAction), nameof(TrackedAction))]
[QueryProperty("EntryId", "EntryId")]
public partial class ActionEntryCreateOrEditViewModel
    : ArisoulMauiBaseViewModel
{
    private ITrackedActionRepository _actionRepository;

    [ObservableProperty]
    TrackedAction _trackedAction;

    [ObservableProperty]
    string _entryId;

    [ObservableProperty]
    ActionEntry _actionEntry;

    public ActionEntryCreateOrEditViewModel(ITrackedActionRepository trackedActionRepository)
    {
        Title = "Create or Edit Action Entry";

        _actionRepository = trackedActionRepository;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(EntryId))
        {
            Guid entryId = Guid.Parse(EntryId);
            if (entryId == Guid.Empty)
            {
                ActionEntry = new ActionEntry
                {
                    TrackedActionId = TrackedAction.Id
                };
            }
            else
            {
                ActionEntry = TrackedAction.Entries.FirstOrDefault(e => e.Id == entryId);
            }
        }
    }

    [RelayCommand]
    async Task SaveAsync()
    {
        if (ActionEntry == null)
            return;

        if (ActionEntry.Id == Guid.Empty) // new
        {
            ActionEntry.Id = Guid.NewGuid();
            ActionEntry.CreatedAt = DateTime.UtcNow;

            await _actionRepository.AddActionEntryAsync(TrackedAction.Id, ActionEntry);
        }
        else // update
        {
            await _actionRepository.UpdateActionEntryAsync(TrackedAction.Id, ActionEntry);
        }

        await Shell.Current.GoToAsync("..");
    }
}
