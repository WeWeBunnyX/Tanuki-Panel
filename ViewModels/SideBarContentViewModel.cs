using System.Collections.ObjectModel;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class SideBarContentViewModel : ViewModelBase
{
    private string _title = "Projects Dashboard";
    private ViewModelBase? _currentViewModel;
    private IGitLabApiService? _gitLabService;
    private INavigationService? _navigationService;
    private IFilePickerService? _filePickerService;
    private IToastService? _toastService;

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

    public void Initialize(IGitLabApiService gitLabService, INavigationService? navigationService = null, IFilePickerService? filePickerService = null, IToastService? toastService = null)
    {
        _gitLabService = gitLabService;
        _navigationService = navigationService;
        _filePickerService = filePickerService;
        _toastService = toastService;
        var projectsVM = new ProjectsViewModel();
        projectsVM.Initialize(_gitLabService, _navigationService, _toastService);
        CurrentViewModel = projectsVM;
        Title = "📊 Projects Dashboard";
    }

    private void OnSelect(string? option)
    {
        switch (option)
        {
            case "Projects": 
                var projectsVM = new ProjectsViewModel();
                projectsVM.Initialize(_gitLabService!, _navigationService, _toastService);
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
                packageVM.Initialize(_gitLabService!, _filePickerService);
                CurrentViewModel = packageVM;
                Title = "📥 Package Registry";
                break;
            case "Issues": 
                var issuesVM = new IssuesViewModel();
                issuesVM.Initialize(_gitLabService!, _navigationService, _toastService);
                CurrentViewModel = issuesVM;
                Title = "📋 Issues";
                break;
            case "Option4": 
                var commitVM = new CommitViewModel();
                commitVM.Initialize(_gitLabService!);
                CurrentViewModel = commitVM;
                Title = "◎ Commit Viewer";
                break;
            case "Option5": 
                CurrentViewModel = new Option5ViewModel();
                Title = "📈 Analytics";
                break;
            default: 
                var defaultProjects = new ProjectsViewModel();
                defaultProjects.Initialize(_gitLabService!, _navigationService, _toastService);
                CurrentViewModel = defaultProjects;
                Title = "📊 Projects Dashboard";
                break;
        }
    }
}

