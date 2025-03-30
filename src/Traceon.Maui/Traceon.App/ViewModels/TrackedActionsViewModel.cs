using Arisoul.Traceon.Maui.Core.Entities;
using Arisoul.Traceon.Maui.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Arisoul.Traceon.App.ViewModels;

public partial class TrackedActionsViewModel : ObservableObject
{
    private readonly ITrackedActionRepository _repository;

    [ObservableProperty]
    private List<TrackedAction> _actions = [];

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private TrackedAction? _selectedAction;

    public TrackedActionsViewModel(ITrackedActionRepository repository)
    {
        _repository = repository;
    }

    internal async Task LoadActionsAsync()
    {
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var all = await _repository.GetAllAsync(userId);

        Actions = [.. all.OrderByDescending(a => a.CreatedAt)];
    }

    [RelayCommand]
    private async Task SaveAction()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        if (SelectedAction == null) // new
        {
            var action = new TrackedAction
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = Name,
                Description = Description,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(action);
        }
        else // update
        {
            SelectedAction.Name = Name;
            SelectedAction.Description = Description;
            await _repository.UpdateAsync(SelectedAction);
        }

        ClearForm();
        await LoadActionsAsync();
    }

    [RelayCommand]
    private async Task DeleteAction()
    {
        if (SelectedAction != null)
        {
            await _repository.DeleteAsync(SelectedAction.Id);
            ClearForm();
            await LoadActionsAsync();
        }
    }

    [RelayCommand]
    private void PrepareNewAction()
    {
        SelectedAction = null;
        Name = string.Empty;
        Description = string.Empty;
    }

    private void ClearForm()
    {
        SelectedAction = null;
        Name = string.Empty;
        Description = string.Empty;
    }

    public void SelectAction(TrackedAction action)
    {
        SelectedAction = action;
        Name = action.Name;
        Description = action.Description ?? string.Empty;
    }
}
