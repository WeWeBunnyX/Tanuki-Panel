using System;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class ApiKeyViewModel : ViewModelBase
{
    private string? _apiKey;
    public string? ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public IRelayCommand SaveCommand { get; }

    private readonly IApiKeyPersistence _persistence;
    private readonly INavigationService _navigationService;
    private readonly IToastService? _toastService;

    public ApiKeyViewModel(IApiKeyPersistence persistence, INavigationService navigationService, IToastService? toastService = null)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _toastService = toastService;
        SaveCommand = new RelayCommand(OnSave);
    }

    private async void OnSave()
    {
        var apiKey = ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("No API key entered. Nothing saved.");
            return;
        }

        if (_persistence.SaveApiKey(apiKey))
        {
            // Create a fresh GitLab API service with the newly saved token
            var freshGitLabService = new GitLabApiService("https://gitlab.com", apiKey);
            
            // Fetch current user info
            var user = await freshGitLabService.GetCurrentUserDetailedAsync();
            
            // Get MainWindow ViewModel and set user info
            var mainWindowViewModel = _navigationService.GetMainWindowViewModel();
            if (mainWindowViewModel != null && user != null)
            {
                mainWindowViewModel.SetCurrentUser(user);
                Console.WriteLine($"[ViewModel] ApiKey - User loaded: {user.Name}");
            }
            
            var sidebarVM = new SideBarContentViewModel();
            sidebarVM.Initialize(freshGitLabService, _navigationService, null, _toastService);
            _navigationService.Navigate(sidebarVM);
        }
    }
}
