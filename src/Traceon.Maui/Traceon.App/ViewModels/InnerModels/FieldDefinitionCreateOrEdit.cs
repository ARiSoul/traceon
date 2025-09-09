using Arisoul.Core.Root.Models.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Arisoul.Traceon.App.ViewModels.InnerModels;

public partial class FieldDefinitionCreateOrEdit
    : BaseObservableValidator
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDropdownTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsIntegerTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsDecimalTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsTextTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsBooleanTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsDateTypeSelected))]
    Maui.Core.Entities.FieldType _selectedFieldType;

    [ObservableProperty] int? _defaultIntegerMaxValue;
    [ObservableProperty] int? _defaultIntegerMinValue;
    [ObservableProperty] decimal? _defaultDecimalMaxValue;
    [ObservableProperty] decimal? _defaultDecimalMinValue;
    [ObservableProperty] bool _nameHasError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DropdownValuesList))]
    string _dropDownValuesAsString;

    public List<string> DropdownValuesList => !string.IsNullOrWhiteSpace(DropDownValuesAsString)
      ? [.. DropDownValuesAsString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]
      : [];

    public bool IsDropdownTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Dropdown;
    public bool IsIntegerTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Integer;
    public bool IsDecimalTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Decimal;
    public bool IsTextTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Text;
    public bool IsBooleanTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Boolean;
    public bool IsDateTypeSelected => SelectedFieldType == Maui.Core.Entities.FieldType.Date;

    public ObservableCollection<Maui.Core.Entities.FieldType> FieldTypes { get; private set; } = [];

    public void LoadFieldTypes()
    {
        FieldTypes = [.. Enum.GetValues<Maui.Core.Entities.FieldType>().Cast<Maui.Core.Entities.FieldType>()];
    }
}
