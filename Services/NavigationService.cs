using System;
using System.Collections.Generic;
using TanukiPanel.ViewModels;

namespace TanukiPanel.Services;

/// <summary>
/// Implements navigation by updating the main window's current view model.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly Stack<ViewModelBase> _navigationHistory = new();
    private ViewModelBase? _currentViewModel;

    public bool CanGoBack => _navigationHistory.Count > 0;

    public NavigationService(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
        _currentViewModel = mainWindowViewModel.CurrentViewModel;
    }

    public void Navigate(ViewModelBase viewModel)
    {
        Console.WriteLine($"[NavigationService] Navigate called. Current: {_currentViewModel?.GetType().Name}, New: {viewModel.GetType().Name}");
        
        // Update _currentViewModel from MainWindowViewModel first (in case it was set externally)
        if (_mainWindowViewModel.CurrentViewModel != null)
        {
            _currentViewModel = _mainWindowViewModel.CurrentViewModel;
        }
        
        // Only add to history if we're navigating away from the current view
        if (_currentViewModel != null && _currentViewModel != viewModel)
        {
            Console.WriteLine($"[NavigationService] Pushing {_currentViewModel.GetType().Name} to history");
            _navigationHistory.Push(_currentViewModel);
        }
        
        _currentViewModel = viewModel;
        _mainWindowViewModel.CurrentViewModel = viewModel;
        Console.WriteLine($"[NavigationService] History count after navigate: {_navigationHistory.Count}");
    }
    
    public void GoBack()
    {
        Console.WriteLine($"[NavigationService] GoBack called. History count: {_navigationHistory.Count}");
        if (_navigationHistory.Count > 0)
        {
            _currentViewModel = _navigationHistory.Pop();
            _mainWindowViewModel.CurrentViewModel = _currentViewModel;
            Console.WriteLine($"[NavigationService] Navigated back to {_currentViewModel?.GetType().Name}");
        }
        else
        {
            Console.WriteLine("[NavigationService] ERROR: No history to go back to!");
        }
    }

    public MainWindowViewModel? GetMainWindowViewModel()
    {
        return _mainWindowViewModel;
    }
}
