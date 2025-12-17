using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class ApiKeyGuideViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    public ApiKeyGuideViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public ICommand GoBackCommand => new RelayCommand(GoBack);

    private void GoBack()
    {
        Console.WriteLine($"[ApiKeyGuideViewModel] GoBack called. CanGoBack: {_navigationService.CanGoBack}");
        if (_navigationService.CanGoBack)
        {
            Console.WriteLine("[ApiKeyGuideViewModel] Calling NavigationService.GoBack()");
            _navigationService.GoBack();
        }
        else
        {
            Console.WriteLine("[ApiKeyGuideViewModel] ERROR: Cannot go back!");
        }
    }
}

