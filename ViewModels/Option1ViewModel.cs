using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class Option1ViewModel : ViewModelBase
{
    private ObservableCollection<Project> _projects = new();
    private string _loadingMessage = "Loading projects...";
    private bool _isLoading = true;
    private string _filterVisibility = "All";
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

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<Project> OpenProjectCommand { get; }

    public Option1ViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(LoadProjectsAsync);
        OpenProjectCommand = new RelayCommand<Project>(OpenProject);
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
            
            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }

            if (Projects.Count == 0)
            {
                LoadingMessage = "No projects found";
            }
            else
            {
                LoadingMessage = $"Loaded {Projects.Count} projects";
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
        // Filter logic can be enhanced here if needed
    }

    private void OpenProject(Project? project)
    {
        if (project == null) return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = project.WebUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open project: {ex.Message}");
        }
    }
}

