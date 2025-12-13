using System;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private ViewModelBase? _currentViewModel;

	public ViewModelBase? CurrentViewModel
	{
		get => _currentViewModel;
		set => SetProperty(ref _currentViewModel, value);
	}

	private readonly IApiKeyPersistence _persistence;

	public MainWindowViewModel(IApiKeyPersistence persistence)
	{
		_persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
		// Don't initialize CurrentViewModel here; it will be set via NavigationService later
	}

	// Called by NavigationService after DI is fully set up
	public void InitializeWithNavigation(INavigationService navigationService)
	{
		var welcomeViewModel = new WelcomeViewModel(navigationService, _persistence);
		CurrentViewModel = welcomeViewModel;
	}
}
