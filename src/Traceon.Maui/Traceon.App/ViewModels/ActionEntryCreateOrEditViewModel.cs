using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace Arisoul.Traceon.App.ViewModels;

[QueryProperty(nameof(ActionEntry), nameof(ActionEntry))]
public partial class ActionEntryCreateOrEditViewModel
    : ArisoulMauiBaseViewModel
{
    private IActionEntryRepository _actionEntryRepository;

    [ObservableProperty]
    ActionEntry _actionEntry;

    [ObservableProperty]
    DateOnly _entryDate;

    [ObservableProperty]
    TimeOnly _entryTime;

    public ActionEntryCreateOrEditViewModel(IActionEntryRepository trackedActionRepository)
    {
        Title = "Create or Edit Action Entry";

        _actionEntryRepository = trackedActionRepository;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActionEntry))
        {
            EntryDate = ActionEntry?.Timestamp is null ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(ActionEntry.Timestamp.Value);
            EntryTime = ActionEntry?.Timestamp is null ? TimeOnly.FromDateTime(DateTime.UtcNow) : TimeOnly.FromDateTime(ActionEntry.Timestamp.Value);
        }
    }

    [RelayCommand]
    async Task SaveAsync()
    {
        if (ActionEntry == null)
            return;

        ActionEntry.Timestamp = new DateTime(EntryDate.Year, EntryDate.Month, EntryDate.Day, EntryTime.Hour, EntryTime.Minute, EntryTime.Second);

        if (ActionEntry.Id == Guid.Empty) // new
        {
            ActionEntry.Id = Guid.NewGuid();
            ActionEntry.CreatedAt = DateTime.UtcNow;

            await _actionEntryRepository.AddAsync(ActionEntry);
        }
        else // update
        {
            await _actionEntryRepository.UpdateAsync(ActionEntry);
        }

        await Shell.Current.GoToAsync("..");
    }
}
