using System;
using TanukiPanel.ViewModels;

namespace TanukiPanel.Services;

/// <summary>
/// Implements navigation by updating the main window's current view model.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public NavigationService(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
    }

    public void Navigate(ViewModelBase viewModel)
    {
        _mainWindowViewModel.CurrentViewModel = viewModel;
    }
}
