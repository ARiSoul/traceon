using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Models;
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
        _unitOfWork = unitOfWork;

        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TrackedAction))
                if (TrackedAction != null)
                    SetTitle();
        };
    }

    private void SetTitle()
    {
        if (TrackedAction == null)
            return;

        string actionName = $"{Arisoul.Localization.Strings.Messages.Create}";
        if (ActionEntry != null && ActionEntry.Id != Guid.Empty)
            actionName = $"{Arisoul.Localization.Strings.Messages.Edit}";

        Title = $"{actionName} - {Localization.Strings.ActionEntry} in action {TrackedAction.Name}";
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
                    ActionId = TrackedAction.Id,
                    Timestamp = DateTime.Now,
                };

                foreach (var actionField in TrackedAction.Fields)
                {
                    ActionEntry.Fields.Add(new ActionEntryField
                    {
                        FieldDefinitionId = actionField.FieldDefinitionId,
                        FieldDefinition = actionField.FieldDefinition,
                        ActionEntryId = ActionEntry.Id,
                        ActionField = actionField,
                        ActionFieldId = actionField.Id,
                        Value = string.Empty
                    });
                }
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

        if (!await ValidateRequiredFieldsAsync())
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

    private async Task<bool> ValidateRequiredFieldsAsync()
    {
        IList<string> requiredFields = [];
        foreach (var field in ActionEntry.Fields)
            if (field.ActionField.IsRequired && string.IsNullOrWhiteSpace(field.Value))
                requiredFields.Add(field.ActionField.Name);

        if (requiredFields.Count > 0)
        {
            await this.Dialogs.ShowError(string.Format(Localization.Strings.ERROR_TheFollowingFieldsAreRequired, string.Join(", ", requiredFields)), Localization.Strings.ERROR_RequiredFieldsTitle);

            return false;
        }

        return true;
    }
}
