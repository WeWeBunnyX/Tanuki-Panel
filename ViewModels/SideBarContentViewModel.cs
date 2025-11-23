using System.Collections.ObjectModel;

namespace TanukiPanel.ViewModels;

public class SideBarContentViewModel : ViewModelBase
{
    private string _title = "Main Dashboard";
    private string? _apiKey;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public ObservableCollection<string> MenuItems { get; } = new ObservableCollection<string>
    {
        "Option 1",
        "Option 2",
        "Option 3",
        "Option 4",
        "Option 5",
    };

    public string? ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public SideBarContentViewModel(string? apiKey = null)
    {
        ApiKey = apiKey ?? "(no API key)";
    }
}
