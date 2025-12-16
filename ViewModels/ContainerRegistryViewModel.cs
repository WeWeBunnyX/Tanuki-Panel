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
        if (_gitLabService == null || string.IsNullOrWhiteSpace(RepositoryPath))
        {
            LoadingMessage = "Please enter a repository path";
            return;
        }

        IsLoading = true;
        LoadingMessage = $"Loading repository: {RepositoryPath}...";
        Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Loading: {RepositoryPath}");

        try
        {
            var project = await _gitLabService.GetProjectByPathAsync(RepositoryPath);
            if (project == null)
            {
                LoadingMessage = $"Project not found: {RepositoryPath}";
                Registries.Clear();
                Tags.Clear();
                return;
            }

            SelectedProject = project;
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Found project: {project.Name} (ID: {project.Id})");

            var registries = await _gitLabService.GetRegistryRepositoriesAsync(project.Id);
            Registries.Clear();
            foreach (var registry in registries)
            {
                Registries.Add(registry);
            }

            LoadingMessage = registries.Count > 0
                ? $"Found {registries.Count} registr{(registries.Count == 1 ? "y" : "ies")} for {project.Name}"
                : $"No registries found for {project.Name}";

            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Found {registries.Count} registries");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - ERROR: {ex.Message}");
            LoadingMessage = $"Error: {ex.Message}";
            Registries.Clear();
            Tags.Clear();
        }
        finally
        {
            IsLoading = false;
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
