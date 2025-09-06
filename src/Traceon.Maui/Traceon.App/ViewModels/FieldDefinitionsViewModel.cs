using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.App.Messages;
using Arisoul.Traceon.Maui.Core.Models;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Arisoul.Traceon.App.ViewModels;

public partial class FieldDefinitionsViewModel : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private List<FieldDefinition> _allFieldDefinitions = [];
    
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private FieldDefinition? _selectedFieldDefinition;
    [ObservableProperty] private bool _isSelectionMode;
    [ObservableProperty] private List<Guid> _fieldsToHide = [];

    public FieldDefinitionsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public ObservableCollection<FieldDefinition> FieldDefinitions { get; private set; } = [];
    
    internal async Task LoadFieldDefinitionsAsync()
    {
        var getAllResult = await _unitOfWork.FieldDefinitions.GetAllAsync(asNoTracking: true);

        if (getAllResult.Failed)
        {
            await this.Dialogs.ShowError(Localization.Strings.ErrorLoadingData);
            return;
        }

        var fields = getAllResult.Value!;

        if (FieldsToHide.Count > 0)
        {
            fields = [.. fields.Where(fd => !FieldsToHide.Any(fth => fth.Equals(fd.Id)))];
        }

        _allFieldDefinitions = [.. fields.OrderBy(a => a.DefaultName)];
        Search(SearchQuery);
    }
    
    [RelayCommand]
    private async Task DeleteFieldDefinitionAsync(FieldDefinition fieldDefinition)
    {
        if (fieldDefinition is null)
            return;

        await _unitOfWork.FieldDefinitions.DeleteAsync(fieldDefinition.Id);
        await _unitOfWork.SaveChangesAsync();
        await LoadFieldDefinitionsAsync();
    }

    [RelayCommand]
    private async Task CreateOrEditFieldDefinitionAsync(FieldDefinition? fieldDefinition)
    {
        fieldDefinition ??= new FieldDefinition();

        await Shell.Current.GoToAsync(nameof(Views.FieldDefinitionCreateOrEditPage), true, new Dictionary<string, object>
        {
            { nameof(FieldDefinition), fieldDefinition }
        });
    }

    [RelayCommand]
    private void Search(string query)
    {
        List<FieldDefinition> fields = [];

        FieldDefinitions.Clear();

        if (string.IsNullOrWhiteSpace(query))
            fields = [.. _allFieldDefinitions.OrderBy(x => x.DefaultName)];
        else
        {
            fields = [.. _allFieldDefinitions
                .Where(x => x.DefaultName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (x.DefaultDescription is not null && x.DefaultDescription.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(x => x.DefaultName)];
        }

        foreach (var action in fields)
            FieldDefinitions.Add(action);
    }

    [RelayCommand]
    private async Task HandleSelectionAsync()
    {
        if (SelectedFieldDefinition is not null)
        {
            await HandleSelectionCoreAsync(SelectedFieldDefinition);
        }
    }

    [RelayCommand]
    private async Task HandleSelectedFieldDefinitionAsync(FieldDefinition field)
    {
        if (field is not null)
        {
            await HandleSelectionCoreAsync(field);
        }
    }

    private async Task HandleSelectionCoreAsync(FieldDefinition field)
    {
        if (IsSelectionMode)
        {
            WeakReferenceMessenger.Default.Send(new FieldDefinitionSelectedMessage(field));
            await Shell.Current.GoToAsync("..");
        }
        else
            await CreateOrEditFieldDefinitionAsync(field);
    }
}
