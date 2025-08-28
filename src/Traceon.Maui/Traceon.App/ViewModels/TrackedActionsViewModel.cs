using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Arisoul.Traceon.App.ViewModels;

public partial class TrackedActionsViewModel : ArisoulMauiBaseViewModel
{
    private readonly IUnitOfWork _unitOfWork;
    private List<TrackedAction> _allActions = [];

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private TrackedAction? _selectedAction;

    public TrackedActionsViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public ObservableCollection<TrackedAction> Actions { get; private set; } = [];

    internal async Task LoadActionsAsync()
    {
        var all = await _unitOfWork.TrackedActions.GetAllAsync();

        _allActions = [.. all.OrderBy(a => a.Name)];

        Search(SearchQuery);
    }

    [RelayCommand]
    private async Task DeleteActionAsync(TrackedAction action)
    {
        if (action is null)
            return;

        await _unitOfWork.TrackedActions.DeleteAsync(action.Id);
        await _unitOfWork.SaveChangesAsync();

        await LoadActionsAsync();
    }

    [RelayCommand]
    private async Task CreateOrEditActionAsync(TrackedAction? action)
    {
        action ??= new TrackedAction();

        await Shell.Current.GoToAsync(nameof(Views.TrackedActionCreateOrEditPage), true, new Dictionary<string, object>
        {
            { nameof(TrackedAction), action }
        });
    }

    [RelayCommand]
    private void Search(string query)
    {
        List<TrackedAction> actions = [];

        Actions.Clear();

        if (string.IsNullOrWhiteSpace(query))
            actions = [.. _allActions.OrderBy(x => x.Name)];
        else
        {
            actions = [.. _allActions
                .Where(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (x.Description is not null && x.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(x => x.Name)];
        }

        foreach (var action in actions)
            Actions.Add(action);
    }

    [RelayCommand]
    private async Task HandleSelectionAsync()
    {
        if (SelectedAction is not null)
        {
            await HandleSelectionCoreAsync(SelectedAction);
        }
    }

    [RelayCommand]
    private async Task HandleSelectedActionAsync(TrackedAction action)
    {
        if (action is not null)
        {
            await HandleSelectionCoreAsync(action);
        }
    }

    private async Task HandleSelectionCoreAsync(TrackedAction action)
    {
        var parameters = new Dictionary<string, object>
            {
                { nameof(TrackedAction), action },
                { "EntryId", Guid.Empty.ToString() }
            };

        await Shell.Current.GoToAsync(nameof(Views.ActionEntryCreateOrEditPage), true, parameters).ConfigureAwait(false);
    }
}
