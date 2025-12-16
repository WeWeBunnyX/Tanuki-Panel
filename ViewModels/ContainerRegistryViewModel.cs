using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class ContainerRegistryViewModel : ViewModelBase
{
    private ObservableCollection<RegistryRepository> _registries = new();
    private ObservableCollection<RegistryTag> _tags = new();
    private ObservableCollection<Package> _packages = new();
    private string _repositoryPath = "";
    private string _loadingMessage = "Ready";
    private bool _isLoading = false;
    private IGitLabApiService? _gitLabService;
    private Project? _selectedProject;
    private RegistryRepository? _selectedRegistry;
    private string _tagLogs = "";
    private bool _showLogs = false;

    public ObservableCollection<RegistryRepository> Registries
    {
        get => _registries;
        set => SetProperty(ref _registries, value);
    }

    public ObservableCollection<RegistryTag> Tags
    {
        get => _tags;
        set => SetProperty(ref _tags, value);
    }

    public ObservableCollection<Package> Packages
    {
        get => _packages;
        set => SetProperty(ref _packages, value);
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

    public RegistryRepository? SelectedRegistry
    {
        get => _selectedRegistry;
        set
        {
            if (SetProperty(ref _selectedRegistry, value))
            {
                if (value != null && SelectedProject != null)
                {
                    _ = LoadRegistryTagsAsync();
                }
            }
        }
    }

    public string TagLogs
    {
        get => _tagLogs;
        set => SetProperty(ref _tagLogs, value);
    }

    public bool ShowLogs
    {
        get => _showLogs;
        set => SetProperty(ref _showLogs, value);
    }

    public IRelayCommand LoadRepositoryCommand { get; }
    public IRelayCommand<RegistryTag> ViewTagLogsCommand { get; }
    public IRelayCommand<RegistryTag> DeleteTagCommand { get; }
    public IRelayCommand RefreshCommand { get; }

    public ContainerRegistryViewModel()
    {
        LoadRepositoryCommand = new AsyncRelayCommand(LoadRepositoryAsync);
        ViewTagLogsCommand = new AsyncRelayCommand<RegistryTag>(ViewTagLogsAsync);
        DeleteTagCommand = new AsyncRelayCommand<RegistryTag>(DeleteTagAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public void Initialize(IGitLabApiService gitLabService)
    {
        _gitLabService = gitLabService;
        LoadingMessage = "Container Registry Ready";
    }

    private async Task LoadRepositoryAsync()
    {
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync called - RepositoryPath: '{RepositoryPath}'");
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync - _gitLabService is null: {_gitLabService == null}");
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync - RepositoryPath is empty/whitespace: {string.IsNullOrWhiteSpace(RepositoryPath)}");
        
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
                // Extract path and remove leading slash
                projectPath = uri.AbsolutePath.TrimStart('/');
                // Remove .git suffix if present
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
                Registries.Clear();
                Tags.Clear();
                Packages.Clear();
                return;
            }

            SelectedProject = project;
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Found project: {project.Name} (ID: {project.Id})");

            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Calling GetRegistryRepositoriesAsync with project ID: {project.Id}");
            var registries = await _gitLabService.GetRegistryRepositoriesAsync(project.Id);
            
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Received {registries.Count} registries");
            Registries.Clear();
            foreach (var registry in registries)
            {
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Adding registry: {registry.Name} (ID: {registry.Id})");
                Registries.Add(registry);
            }

            // Also load packages from Package Registry
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Calling GetPackagesAsync with project ID: {project.Id}");
            var packages = await _gitLabService.GetPackagesAsync(project.Id);
            
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Received {packages.Count} packages");
            Packages.Clear();
            foreach (var package in packages)
            {
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Adding package: {package.Name} v{package.Version} ({package.PackageType})");
                Packages.Add(package);
            }

            int totalItems = registries.Count + packages.Count;
            string message = $"Found {registries.Count} container image(s) and {packages.Count} package(s)";
            if (totalItems == 0)
            {
                message = $"No container images or packages found for {project.Name}";
            }
            LoadingMessage = message;
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - {message}");

            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Complete. Found {registries.Count} registries");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Stack trace: {ex.StackTrace}");
            LoadingMessage = $"Error: {ex.Message}";
            Registries.Clear();
            Tags.Clear();
            Packages.Clear();
        }
        finally
        {
            IsLoading = false;
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Done. IsLoading={IsLoading}, Message={LoadingMessage}");
        }
    }

    private async Task LoadRegistryTagsAsync()
    {
        if (_gitLabService == null || SelectedProject == null || SelectedRegistry == null)
        {
            return;
        }

        IsLoading = true;
        LoadingMessage = $"Loading tags for {SelectedRegistry.Name}...";
        Console.WriteLine($"[ViewModel] LoadRegistryTagsAsync - Loading tags for registry: {SelectedRegistry.Name}");

        try
        {
            var tags = await _gitLabService.GetRegistryTagsAsync(SelectedProject.Id, SelectedRegistry.Id);
            Tags.Clear();
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }

            LoadingMessage = tags.Count > 0
                ? $"Found {tags.Count} tag{(tags.Count == 1 ? "" : "s")}"
                : "No tags found";

            Console.WriteLine($"[ViewModel] LoadRegistryTagsAsync - Found {tags.Count} tags");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] LoadRegistryTagsAsync - ERROR: {ex.Message}");
            LoadingMessage = $"Error loading tags: {ex.Message}";
            Tags.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewTagLogsAsync(RegistryTag? tag)
    {
        if (tag == null || _gitLabService == null || SelectedProject == null || SelectedRegistry == null)
        {
            return;
        }

        IsLoading = true;
        LoadingMessage = $"Fetching logs for tag: {tag.Name}...";
        Console.WriteLine($"[ViewModel] ViewTagLogsAsync - Fetching logs for tag: {tag.Name}");

        try
        {
            TagLogs = await _gitLabService.GetRegistryTagLogsAsync(SelectedProject.Id, SelectedRegistry.Id, tag.Name);
            ShowLogs = true;
            LoadingMessage = "Logs loaded";
            Console.WriteLine($"[ViewModel] ViewTagLogsAsync - Successfully loaded logs");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] ViewTagLogsAsync - ERROR: {ex.Message}");
            TagLogs = $"Error fetching logs: {ex.Message}";
            ShowLogs = true;
            LoadingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task DeleteTagAsync(RegistryTag? tag)
    {
        if (tag == null || _gitLabService == null || SelectedProject == null || SelectedRegistry == null)
        {
            return;
        }

        IsLoading = true;
        LoadingMessage = $"Deleting tag: {tag.Name}...";
        Console.WriteLine($"[ViewModel] DeleteTagAsync - Deleting tag: {tag.Name}");

        try
        {
            var success = await _gitLabService.DeleteRegistryTagAsync(SelectedProject.Id, SelectedRegistry.Id, tag.Name);
            if (success)
            {
                Tags.Remove(tag);
                LoadingMessage = $"Successfully deleted tag: {tag.Name}";
                Console.WriteLine($"[ViewModel] DeleteTagAsync - Successfully deleted tag");
                await RefreshAsync();
            }
            else
            {
                LoadingMessage = $"Failed to delete tag: {tag.Name}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] DeleteTagAsync - ERROR: {ex.Message}");
            LoadingMessage = $"Error deleting tag: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        if (SelectedProject != null && SelectedRegistry != null)
        {
            await LoadRegistryTagsAsync();
        }
    }
}
