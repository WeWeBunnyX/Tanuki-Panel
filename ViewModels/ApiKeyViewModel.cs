
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

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

    public ApiKeyViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
    }

    private void OnSave()
    {
        // TODO
    }
}
