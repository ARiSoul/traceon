using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.App.Messages;
using Arisoul.Traceon.Maui.Core.Models;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;

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
        _unitOfWork = unitOfWork;

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TrackedAction))
                if (TrackedAction != null)
                    SetTitle();
        };

        WeakReferenceMessenger.Default.Register<FieldDefinitionSelectedMessage>(this, (r, m) =>
        {
            var selected = m.Value;
            TrackedAction.Fields.Add(new ActionField
            {
                Description = selected.DefaultDescription,
                FieldDefinitionId = selected.Id,
                IsRequired = selected.DefaultIsRequired,
                MaxValue = selected.DefaultMaxValue,
                MinValue = selected.DefaultMinValue,
                Name = selected.DefaultName,
                FieldDefinition = selected
            });
        });
    }

    private void SetTitle()
    {
        string actionName = $"{Arisoul.Localization.Strings.Messages.Create}";
        if (TrackedAction.Id != Guid.Empty)
            actionName = $"{Arisoul.Localization.Strings.Messages.Edit}";

        Title = $"{actionName} - {Localization.Strings.Action}";
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

    [RelayCommand]
    void AddActionField()
    {
        if (TrackedAction == null)
            return;

        var fieldsToHide = TrackedAction.Fields.Select(f => f.FieldDefinitionId).ToList();
        WeakReferenceMessenger.Default.Send(new NavigateToFieldDefinitionsMessage(_unitOfWork, true, fieldsToHide));
    }

    [RelayCommand]
    async Task DeleteActionField(ActionField actionField)
    {
        if (actionField is null)
            return;

        TrackedAction.Fields.Remove(actionField);

        await _unitOfWork.ActionFields.DeleteAsync(actionField.ActionId, actionField.FieldDefinitionId).ConfigureAwait(false);
    }
}
