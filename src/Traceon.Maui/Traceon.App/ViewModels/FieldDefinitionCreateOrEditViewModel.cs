using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.App.ViewModels.InnerModels;
using Arisoul.Traceon.Maui.Core.Interfaces;
using Arisoul.Traceon.Maui.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arisoul.Traceon.App.ViewModels;

[QueryProperty(nameof(FieldDefinition), nameof(FieldDefinition))]
public partial class FieldDefinitionCreateOrEditViewModel
    : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty] FieldDefinition _fieldDefinition;

    public FieldDefinitionCreateOrEdit InnerModel { get; private set; } = new();

    public FieldDefinitionCreateOrEditViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        InnerModel.LoadFieldTypes();

        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FieldDefinition))
            {
                if (FieldDefinition != null)
                {
                    InnerModel.DefaultIntegerMaxValue = (int?)FieldDefinition.DefaultMaxValue;
                    InnerModel.DefaultIntegerMinValue = (int?)FieldDefinition.DefaultMinValue;
                    InnerModel.DefaultDecimalMaxValue = FieldDefinition.DefaultMaxValue;
                    InnerModel.DefaultDecimalMinValue = FieldDefinition.DefaultMinValue;
                    InnerModel.SelectedFieldType = FieldDefinition.Type;
                    InnerModel.DropDownValuesAsString = string.IsNullOrWhiteSpace(FieldDefinition.DropdownValues) 
                    ? string.Empty 
                    : FieldDefinition.DropdownValues.Replace(",", ";");
                    SetTitle();
                }
            }
        };
    }

    private void SetTitle()
    {
        if (FieldDefinition == null)
            return;

        string actionName = $"{Arisoul.Localization.Strings.Messages.Create}";
        if (FieldDefinition.Id != Guid.Empty)
            actionName = $"{Arisoul.Localization.Strings.Messages.Edit}";

        Title = $"{actionName} - {Localization.Strings.FieldDefinition}";
    }

    [RelayCommand]
    async Task SaveFieldAsync()
    {
        if (FieldDefinition == null)
            return;

        MapViewModelFieldsToEntity();

        if (!ValidateSave())
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

    private void MapViewModelFieldsToEntity()
    {
        FieldDefinition.Type = InnerModel.SelectedFieldType;

        if (InnerModel.IsIntegerTypeSelected)
        {
            FieldDefinition.DefaultMaxValue = InnerModel.DefaultIntegerMaxValue;
            FieldDefinition.DefaultMinValue = InnerModel.DefaultIntegerMinValue;
        }
        else if (InnerModel.IsDecimalTypeSelected)
        {
            FieldDefinition.DefaultMaxValue = InnerModel.DefaultDecimalMaxValue;
            FieldDefinition.DefaultMinValue = InnerModel.DefaultDecimalMinValue;
        }

        if (InnerModel.IsDropdownTypeSelected)
            FieldDefinition.DropdownValues = InnerModel.DropDownValuesAsString?.Replace(";", ",");
        else
            FieldDefinition.DropdownValues = null;
    }

    private bool ValidateSave()
    {
        bool isValid = !string.IsNullOrWhiteSpace(FieldDefinition?.DefaultName);
        InnerModel.NameHasError = !isValid;

        return isValid;
    }
}
