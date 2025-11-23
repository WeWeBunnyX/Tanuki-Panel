using System.Collections.ObjectModel;

namespace TanukiPanel.ViewModels;

public class SideBarContentViewModel : ViewModelBase
{
    private string _title = "Main Dashboard";
    private ViewModelBase? _currentViewModel;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public CommunityToolkit.Mvvm.Input.IRelayCommand<string> SelectCommand { get; }

    public SideBarContentViewModel()
    {
        // default content
        CurrentViewModel = new Option1ViewModel();
        SelectCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<string>(OnSelect);
    }

    private void OnSelect(string option)
    {
        switch (option)
        {
            case "Option1": CurrentViewModel = new Option1ViewModel(); break;
            case "Option2": CurrentViewModel = new Option2ViewModel(); break;
            case "Option3": CurrentViewModel = new Option3ViewModel(); break;
            case "Option4": CurrentViewModel = new Option4ViewModel(); break;
            case "Option5": CurrentViewModel = new Option5ViewModel(); break;
            default: CurrentViewModel = new Option1ViewModel(); break;
        }
    }
}
