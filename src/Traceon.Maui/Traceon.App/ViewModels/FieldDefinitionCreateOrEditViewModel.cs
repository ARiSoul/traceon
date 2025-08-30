using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Models;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Arisoul.Traceon.App.ViewModels;

[QueryProperty(nameof(FieldDefinition), nameof(FieldDefinition))]
public partial class FieldDefinitionCreateOrEditViewModel
    : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDropdownTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsIntegerTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsDecimalTypeSelected))]
    Maui.Core.Entities.FieldType _selectedFieldType;

    [ObservableProperty] FieldDefinition _fieldDefinition;
    [ObservableProperty] int? _defaultIntegerMaxValue;
    [ObservableProperty] int? _defaultIntegerMinValue;
    [ObservableProperty] decimal? _defaultDecimalMaxValue;
    [ObservableProperty] decimal? _defaultDecimalMinValue;

    public bool IsDropdownTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Dropdown;
    public bool IsIntegerTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Integer;
    public bool IsDecimalTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Decimal;

    public ObservableCollection<Maui.Core.Entities.FieldType> FieldTypes { get; private set; } = [];

    public FieldDefinitionCreateOrEditViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        LoadFieldTypes();

        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(FieldDefinition))
            {
                if (FieldDefinition != null)
                {
                    DefaultIntegerMaxValue = (int?)FieldDefinition.DefaultMaxValue;
                    DefaultIntegerMinValue = (int?)FieldDefinition.DefaultMinValue;
                    DefaultDecimalMaxValue = FieldDefinition.DefaultMaxValue;
                    DefaultDecimalMinValue = FieldDefinition.DefaultMinValue;
                    SelectedFieldType = FieldDefinition.Type;
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

        HandleFieldTypeAndMaxAndMinValuesOnSave();

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

    private void HandleFieldTypeAndMaxAndMinValuesOnSave()
    {
        FieldDefinition.Type = SelectedFieldType;

        if (IsIntegerTypeSelected)
        {
            FieldDefinition.DefaultMaxValue = DefaultIntegerMaxValue;
            FieldDefinition.DefaultMinValue = DefaultIntegerMinValue;
        }
        else if (IsDecimalTypeSelected)
        {
            FieldDefinition.DefaultMaxValue = DefaultDecimalMaxValue;
            FieldDefinition.DefaultMinValue = DefaultDecimalMinValue;
        }
    }

    private void LoadFieldTypes()
    {
        FieldTypes = [.. Enum.GetValues<Maui.Core.Entities.FieldType>().Cast<Maui.Core.Entities.FieldType>()];
    }
}
