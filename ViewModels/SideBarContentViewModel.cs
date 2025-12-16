using System.Collections.ObjectModel;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class SideBarContentViewModel : ViewModelBase
{
    private string _title = "Projects Dashboard";
    private ViewModelBase? _currentViewModel;
    private IGitLabApiService? _gitLabService;
    private INavigationService? _navigationService;

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

    public void Initialize(IGitLabApiService gitLabService, INavigationService? navigationService = null)
    {
        _gitLabService = gitLabService;
        _navigationService = navigationService;
        var projectsVM = new ProjectsViewModel();
        projectsVM.Initialize(_gitLabService, _navigationService);
        CurrentViewModel = projectsVM;
        Title = "📊 Projects Dashboard";
    }

    private void OnSelect(string? option)
    {
        switch (option)
        {
            case "Projects": 
                var projectsVM = new ProjectsViewModel();
                projectsVM.Initialize(_gitLabService!, _navigationService);
                CurrentViewModel = projectsVM;
                Title = "📊 Projects Dashboard";
                break;
            case "Option2": 
                var registryVM = new ContainerRegistryViewModel();
                registryVM.Initialize(_gitLabService!);
                CurrentViewModel = registryVM;
                Title = "📦 Container Registry";
                break;
            case "Option3":
                var packageVM = new PackageRegistryViewModel();
                packageVM.Initialize(_gitLabService!);
                CurrentViewModel = packageVM;
                Title = "📥 Package Registry";
                break;
            case "Issues": 
                var issuesVM = new IssuesViewModel();
                issuesVM.Initialize(_gitLabService!, _navigationService);
                CurrentViewModel = issuesVM;
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
                var defaultProjects = new ProjectsViewModel();
                defaultProjects.Initialize(_gitLabService!, _navigationService);
                CurrentViewModel = defaultProjects;
                Title = "📊 Projects Dashboard";
                break;
        }
    }
}

