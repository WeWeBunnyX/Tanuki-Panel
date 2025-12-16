using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class PackageRegistryViewModel : ViewModelBase
{
    private ObservableCollection<Package> _packages = new();
    private string _repositoryPath = "";
    private string _loadingMessage = "Ready";
    private bool _isLoading = false;
    private IGitLabApiService? _gitLabService;
    private Project? _selectedProject;
    private Package? _selectedPackage;

    public ObservableCollection<Package> Packages
    {
        get => _packages;
        set 
        { 
            Console.WriteLine($"[ViewModel] Packages setter called - new count: {value?.Count ?? 0}");
            SetProperty(ref _packages, value);
        }
    }

    public string RepositoryPath
    {
        get => _repositoryPath;
        set => SetProperty(ref _repositoryPath, value);
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Project? SelectedProject
    {
        get => _selectedProject;
        set => SetProperty(ref _selectedProject, value);
    }

    public Package? SelectedPackage
    {
        get => _selectedPackage;
        set => SetProperty(ref _selectedPackage, value);
    }

    public IRelayCommand LoadRepositoryCommand { get; }
    public IRelayCommand<Package> DownloadPackageCommand { get; }
    public IRelayCommand RefreshCommand { get; }

    public PackageRegistryViewModel()
    {
        LoadRepositoryCommand = new AsyncRelayCommand(LoadRepositoryAsync);
        DownloadPackageCommand = new AsyncRelayCommand<Package>(DownloadPackageAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public void Initialize(IGitLabApiService gitLabService)
    {
        _gitLabService = gitLabService;
        LoadingMessage = "Package Registry Ready";
    }

    private async Task LoadRepositoryAsync()
    {
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync called - RepositoryPath: '{RepositoryPath}'");
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync - _gitLabService is null: {_gitLabService == null}");
        
        if (_gitLabService == null)
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - ERROR: GitLabService not initialized");
            LoadingMessage = "Service not initialized";
            return;
        }
        
        if (string.IsNullOrWhiteSpace(RepositoryPath))
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - ERROR: RepositoryPath is empty or whitespace");
            LoadingMessage = "Please enter a repository path";
            return;
        }

        IsLoading = true;
        
        // Extract path from full URL if user entered one
        string projectPath = RepositoryPath;
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Input: {RepositoryPath}");
        
        if (RepositoryPath.StartsWith("http://") || RepositoryPath.StartsWith("https://"))
        {
            try
            {
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Detected full URL, extracting path...");
                var uri = new Uri(RepositoryPath);
                projectPath = uri.AbsolutePath.TrimStart('/');
                if (projectPath.EndsWith(".git"))
                {
                    projectPath = projectPath.Substring(0, projectPath.Length - 4);
                }
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Extracted path from URL: '{projectPath}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - ERROR parsing URL: {ex.Message}");
                LoadingMessage = $"Invalid URL format: {ex.Message}";
                IsLoading = false;
                return;
            }
        }

        LoadingMessage = $"Loading repository: {projectPath}...";
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Final projectPath to use: '{projectPath}'");

        try
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Calling GetProjectByPathAsync with: '{projectPath}'");
            var project = await _gitLabService.GetProjectByPathAsync(projectPath);
            
            if (project == null)
            {
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Project not found for path: {projectPath}");
                LoadingMessage = $"Project not found: {projectPath}";
                Packages.Clear();
                return;
            }

            SelectedProject = project;
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Found project: {project.Name} (ID: {project.Id})");

            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Calling GetPackagesAsync with project ID: {project.Id}");
            var packages = await _gitLabService.GetPackagesAsync(project.Id);
            
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Received {packages.Count} packages");
            Packages.Clear();
            foreach (var pkg in packages)
            {
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Adding package: {pkg.Name} v{pkg.Version} ({pkg.PackageType})");
                Packages.Add(pkg);
            }

            LoadingMessage = packages.Count > 0
                ? $"Found {packages.Count} package{(packages.Count == 1 ? "" : "s")} for {project.Name}"
                : $"No packages found for {project.Name}";

            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Complete. Found {packages.Count} packages");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Stack trace: {ex.StackTrace}");
            LoadingMessage = $"Error: {ex.Message}";
            Packages.Clear();
        }
        finally
        {
            IsLoading = false;
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Done. IsLoading={IsLoading}, Message={LoadingMessage}");
        }
    }

    private async Task DownloadPackageAsync(Package? package)
    {
        if (package == null)
        {
            LoadingMessage = "No package selected";
            return;
        }

        Console.WriteLine($"[ViewModel] DownloadPackageAsync - Downloading package: {package.Name} v{package.Version}");
        LoadingMessage = $"Downloading {package.Name}...";
        
        try
        {
            // TODO: Implement actual download when backend is ready
            Console.WriteLine($"[ViewModel] DownloadPackageAsync - Package download initiated");
            await Task.Delay(500); // Simulate async operation
            LoadingMessage = $"Download started for {package.Name} v{package.Version}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] DownloadPackageAsync - ERROR: {ex.Message}");
            LoadingMessage = $"Download failed: {ex.Message}";
        }
    }

    private async Task RefreshAsync()
    {
        Console.WriteLine($"[ViewModel] RefreshAsync - Refreshing package list");
        await LoadRepositoryAsync();
    }
}
