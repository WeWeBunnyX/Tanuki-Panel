using System.Collections.ObjectModel;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class SideBarContentViewModel : ViewModelBase
{
    private string _title = "Projects Dashboard";
    private ViewModelBase? _currentViewModel;
    private IGitLabApiService? _gitLabService;

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
        SelectCommand = new CommunityToolkit.Mvvm.Input.RelayCommand<string>(OnSelect);
    }

    public void Initialize(IGitLabApiService gitLabService)
    {
        _gitLabService = gitLabService;
        var option1VM = new Option1ViewModel();
        option1VM.Initialize(_gitLabService);
        CurrentViewModel = option1VM;
        Title = "📊 Projects Dashboard";
    }

    private void OnSelect(string? option)
    {
        switch (option)
        {
            case "Option1": 
                var opt1 = new Option1ViewModel();
                opt1.Initialize(_gitLabService!);
                CurrentViewModel = opt1;
                Title = "📊 Projects Dashboard";
                break;
            case "Option2": 
                CurrentViewModel = new Option2ViewModel();
                Title = "🚀 Pipelines";
                break;
            case "Option3": 
                CurrentViewModel = new Option3ViewModel();
                Title = "📋 Issues";
                break;
            case "Option4": 
                CurrentViewModel = new Option4ViewModel();
                Title = "🔧 Settings";
                break;
            case "Option5": 
                CurrentViewModel = new Option5ViewModel();
                Title = "📈 Analytics";
                break;
            default: 
                var defaultOpt = new Option1ViewModel();
                defaultOpt.Initialize(_gitLabService!);
                CurrentViewModel = defaultOpt;
                Title = "📊 Projects Dashboard";
                break;
        }
    }
}

