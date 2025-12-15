using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class ProjectsViewModel : ViewModelBase
{
    private ObservableCollection<Project> _projects = new();
    private List<Project> _allProjects = new(); // Store all projects for filtering
    private string _loadingMessage = "Loading projects...";
    private bool _isLoading = true;
    private string _filterVisibility = "All";
    private string _searchText = "";
    private bool _hideArchived = true;
    private string _sortBy = "LastActivity"; // LastActivity, Name, Stars
    private IGitLabApiService? _gitLabService;

    public ObservableCollection<Project> Projects
    {
        get => _projects;
        set => SetProperty(ref _projects, value);
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

    public string FilterVisibility
    {
        get => _filterVisibility;
        set 
        { 
            if (SetProperty(ref _filterVisibility, value))
            {
                ApplyFilters();
            }
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilters();
            }
        }
    }

    public bool HideArchived
    {
        get => _hideArchived;
        set
        {
            if (SetProperty(ref _hideArchived, value))
            {
                ApplyFilters();
            }
        }
    }

    public string SortBy
    {
        get => _sortBy;
        set
        {
            if (SetProperty(ref _sortBy, value))
            {
                ApplyFilters();
            }
        }
    }

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<Project> OpenProjectCommand { get; }
    public IRelayCommand<Project> CopyCloneUrlCommand { get; }

    public ProjectsViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(LoadProjectsAsync);
        OpenProjectCommand = new RelayCommand<Project>(OpenProject);
        CopyCloneUrlCommand = new RelayCommand<Project>(CopyCloneUrl);
    }

    public void Initialize(IGitLabApiService gitLabService)
    {
        _gitLabService = gitLabService;
        _ = LoadProjectsAsync();
    }

    private async Task LoadProjectsAsync()
    {
        if (_gitLabService == null)
        {
            LoadingMessage = "GitLab service not initialized";
            IsLoading = false;
            return;
        }

        IsLoading = true;
        LoadingMessage = "Fetching projects from GitLab...";

        try
        {
            var projects = await _gitLabService.GetProjectsAsync();
            
            _allProjects = projects;
            ApplyFilters();

            if (Projects.Count == 0)
            {
                LoadingMessage = "No projects found";
            }
            else
            {
                LoadingMessage = $"Loaded {Projects.Count} of {_allProjects.Count} projects";
            }
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allProjects.AsEnumerable();

        // Hide archived if enabled
        if (HideArchived)
        {
            filtered = filtered.Where(p => !p.Archived);
        }

        // Filter by visibility
        if (FilterVisibility != "All")
        {
            filtered = filtered.Where(p => p.Visibility.Equals(FilterVisibility, StringComparison.OrdinalIgnoreCase));
        }

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(p => 
                p.Name.ToLower().Contains(searchLower) || 
                p.Description.ToLower().Contains(searchLower));
        }

        // Sort
        filtered = SortBy switch
        {
            "Name" => filtered.OrderBy(p => p.Name),
            "Stars" => filtered.OrderByDescending(p => p.StarCount),
            "LastActivity" => filtered.OrderByDescending(p => p.LastActivityAt),
            _ => filtered.OrderByDescending(p => p.LastActivityAt)
        };

        Projects.Clear();
        foreach (var project in filtered)
        {
            Projects.Add(project);
        }
    }

    private void OpenProject(Project? project)
    {
        if (project == null) return;

        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true
            };

            // Handle different OS
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                psi.FileName = project.WebUrl;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                psi.FileName = "open";
                psi.Arguments = project.WebUrl;
            }
            else // Linux
            {
                psi.FileName = "xdg-open";
                psi.Arguments = project.WebUrl;
            }

            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open project: {ex.Message}");
        }
    }

    private void CopyCloneUrl(Project? project)
    {
        if (project == null) return;

        try
        {
            // Copy HTTPS URL to clipboard
            // Using xclip for Linux, pbcopy for macOS, or clip for Windows
            var url = $"git clone {project.WebUrl}.git";
            
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // Windows: use clip command
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c echo {url} | clip",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                // macOS: use pbcopy
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"echo '{url}' | pbcopy\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }
            else
            {
                // Linux: use xclip
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"echo -n '{url}' | xclip -selection clipboard\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }

            Console.WriteLine($"Copied clone URL for {project.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to copy URL: {ex.Message}");
        }
    }
}