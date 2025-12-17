using System;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	private ViewModelBase? _currentViewModel;
	private string _currentUserName = "Not logged in";
	private string _currentUserAvatarUrl = "";

	public ViewModelBase? CurrentViewModel
	{
		get => _currentViewModel;
		set => SetProperty(ref _currentViewModel, value);
	}

	public string CurrentUserName
	{
		get => _currentUserName;
		set => SetProperty(ref _currentUserName, value);
	}

	public string CurrentUserAvatarUrl
	{
		get => _currentUserAvatarUrl;
		set => SetProperty(ref _currentUserAvatarUrl, value);
	}

	private readonly IApiKeyPersistence _persistence;
	private readonly IToastService? _toastService;

	public MainWindowViewModel(IApiKeyPersistence persistence, IToastService? toastService = null)
	{
		_persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
		_toastService = toastService;
		// Don't initialize CurrentViewModel here; it will be set via NavigationService later
	}

	// Called by NavigationService after DI is fully set up
	public void InitializeWithNavigation(INavigationService navigationService)
	{
		var welcomeViewModel = new WelcomeViewModel(navigationService, _persistence, _toastService);
		CurrentViewModel = welcomeViewModel;
	}

	public async void SetCurrentUser(User? user)
	{
		if (user != null)
		{
			CurrentUserName = $"{user.Name} (@{user.Username})";
			CurrentUserAvatarUrl = user.AvatarUrl;
			Console.WriteLine($"[ViewModel] MainWindow - User updated: {CurrentUserName}, Avatar: {CurrentUserAvatarUrl}");
		}
		else
		{
			CurrentUserName = "Not logged in";
			CurrentUserAvatarUrl = "";
		}
	}
}
