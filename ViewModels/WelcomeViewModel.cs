using System;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IApiKeyPersistence _persistence;
    private readonly IToastService? _toastService;

    public WelcomeViewModel(INavigationService navigationService, IApiKeyPersistence persistence, IToastService? toastService = null)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _toastService = toastService;
    }

    public void OnAnimationFinished()
    {
        var apiKeyViewModel = new ApiKeyViewModel(_persistence, _navigationService, _toastService);
        _navigationService.Navigate(apiKeyViewModel);
    }
}

