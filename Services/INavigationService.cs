using TanukiPanel.ViewModels;

namespace TanukiPanel.Services;

/// <summary>
/// Service for navigating between views/view models.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified view model.
    /// </summary>
    /// <param name="viewModel">The view model to navigate to.</param>
    void Navigate(ViewModelBase viewModel);
    
    /// <summary>
    /// Goes back to the previous view model.
    /// </summary>
    void GoBack();
    
    /// <summary>
    /// Gets whether there is a previous view to go back to.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets the MainWindow ViewModel.
    /// </summary>
    MainWindowViewModel? GetMainWindowViewModel();
}
