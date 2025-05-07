using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arisoul.Traceon.App.ViewModels;

[QueryProperty(nameof(FieldDefinition), nameof(FieldDefinition))]
public partial class FieldDefinitionCreateOrEditViewModel 
    : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    FieldDefinition _fieldDefinition;

    public FieldDefinitionCreateOrEditViewModel(IUnitOfWork unitOfWork)
    {
        Title = "Create or Edit Field Definition";

        _unitOfWork = unitOfWork;
    }

    [RelayCommand]
    async Task SaveFieldAsync()
    {
        if (FieldDefinition == null)
            return;

        if (FieldDefinition.Id == Guid.Empty) // new
        {
            FieldDefinition.Id = Guid.NewGuid();
            
            await _unitOfWork.FieldDefinitions.CreateAsync(FieldDefinition).ConfigureAwait(false);
        }
        else // update
        {
            await _unitOfWork.FieldDefinitions.UpdateAsync(FieldDefinition).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await Shell.Current.GoToAsync("..");
    }
}
