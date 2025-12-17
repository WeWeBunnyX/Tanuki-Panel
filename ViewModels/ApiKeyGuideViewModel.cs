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
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }
}

