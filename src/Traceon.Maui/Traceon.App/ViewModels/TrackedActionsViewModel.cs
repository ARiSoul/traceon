using Arisoul.Core.Maui.Models;
using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Arisoul.Traceon.App.ViewModels;

public partial class TrackedActionsViewModel
    : ArisoulMauiBaseViewModel
{
    private readonly ITrackedActionRepository _repository;
    private List<TrackedAction> _allActions = [];

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private TrackedAction? _selectedAction;

    public ObservableCollection<TrackedAction> Actions { get; private set; } = [];

    public TrackedActionsViewModel(ITrackedActionRepository repository)
    {
        _repository = repository;
    }

    internal async Task LoadActionsAsync()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var all = await _repository.GetAllAsync(userId);

        _allActions = [.. all.OrderBy(a => a.Name)];

        Actions.Clear();

        foreach (var action in _allActions)
            Actions.Add(action);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            Search(SearchQuery);
    }

    [RelayCommand]
    private async Task DeleteAction(TrackedAction action)
    {
        if (action is null)
            return;

        await _repository.DeleteAsync(action.Id);
        await LoadActionsAsync();
    }

    [RelayCommand]
    private async void CreateOrEditAction(TrackedAction? action)
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
    private async void HandleSelection()
    {
        if (SelectedAction is not null)
        {
            await HandleSelectionCoreAsync(SelectedAction);
        }
    }

    [RelayCommand]
    private async void HandleSelectedAction(TrackedAction action)
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

        // TODO: apply more controls from syncfusion (first check the time and date pickers)

        await Shell.Current.GoToAsync(nameof(Views.ActionEntryCreateOrEditPage), true, parameters);
    }
}
