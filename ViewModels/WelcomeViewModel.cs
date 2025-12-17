using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IApiKeyPersistence _persistence;

    public WelcomeViewModel(INavigationService navigationService, IApiKeyPersistence persistence)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    public ICommand ShowApiKeyGuideCommand => new RelayCommand(ShowApiKeyGuide);

    private void ShowApiKeyGuide()
    {
        var guideViewModel = new ApiKeyGuideViewModel(_navigationService);
        _navigationService.Navigate(guideViewModel);
    }

    public void OnAnimationFinished()
    {
        var apiKeyViewModel = new ApiKeyViewModel(_persistence, _navigationService);
        _navigationService.Navigate(apiKeyViewModel);
    }
}

