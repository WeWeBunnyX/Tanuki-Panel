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
}
