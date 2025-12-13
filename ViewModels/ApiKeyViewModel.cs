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

    public ApiKeyViewModel(IApiKeyPersistence persistence, INavigationService navigationService)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        SaveCommand = new RelayCommand(OnSave);
    }

    private void OnSave()
    {
        var apiKey = ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("No API key entered. Nothing saved.");
            return;
        }

        if (_persistence.SaveApiKey(apiKey))
        {
            _navigationService.Navigate(new SideBarContentViewModel());
        }
    }
}
