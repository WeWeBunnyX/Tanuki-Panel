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
        // Only add to history if we're navigating away from the current view
        if (_currentViewModel != null && _currentViewModel != viewModel)
        {
            _navigationHistory.Push(_currentViewModel);
        }
        
        _currentViewModel = viewModel;
        _mainWindowViewModel.CurrentViewModel = viewModel;
    }
    
    public void GoBack()
    {
        if (_navigationHistory.Count > 0)
        {
            _currentViewModel = _navigationHistory.Pop();
            _mainWindowViewModel.CurrentViewModel = _currentViewModel;
        }
    }
}
