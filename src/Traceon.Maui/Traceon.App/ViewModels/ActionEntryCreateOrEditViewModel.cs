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
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    TrackedAction _trackedAction;

    [ObservableProperty]
    string _entryId;

    [ObservableProperty]
    ActionEntry _actionEntry;

    public ActionEntryCreateOrEditViewModel(IUnitOfWork unitOfWork)
    {
        Title = "Create or Edit Action Entry";

        _unitOfWork = unitOfWork;
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
                    ActionId = TrackedAction.Id
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

            await _unitOfWork.TrackedActions.AddActionEntryAsync(TrackedAction.Id, ActionEntry).ConfigureAwait(false);
        }
        else // update
        {
            await _unitOfWork.TrackedActions.UpdateActionEntryAsync(TrackedAction.Id, ActionEntry).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await Shell.Current.GoToAsync("..");
    }
}
