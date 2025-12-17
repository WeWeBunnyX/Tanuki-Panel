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
    private INavigationService? _navigationService;
    private string _viewMode = "MyProjects"; // "MyProjects" or "SearchProjects"
    private string _searchQuery = "";
    private bool _isSearching = false;
    
    // Pagination
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int _totalSearchResults = 0;
    private string _paginationInfo = "";
    private int _myProjectsPage = 1;
    private const int MyProjectsPageSize = 15;

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
                if (_viewMode == "MyProjects")
                {
                    ApplyFilters();
                }
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

    public string ViewMode
    {
        get => _viewMode;
        set
        {
            if (SetProperty(ref _viewMode, value))
            {
                SearchText = "";
                SearchQuery = "";
                if (value == "MyProjects")
                {
                    ApplyFilters();
                }
                else
                {
                    Projects.Clear();
                }
            }
        }
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        set => SetProperty(ref _isSearching, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    public int TotalSearchResults
    {
        get => _totalSearchResults;
        set => SetProperty(ref _totalSearchResults, value);
    }

    public string PaginationInfo
    {
        get => _paginationInfo;
        set => SetProperty(ref _paginationInfo, value);
    }

    public int MyProjectsPage
    {
        get => _myProjectsPage;
        set => SetProperty(ref _myProjectsPage, value);
    }

    public bool HasPreviousMyPage => MyProjectsPage > 1;
    public bool HasNextMyPage => MyProjectsPage * MyProjectsPageSize < _allProjects.Count;
    public string MyProjectsPageInfo => $"Page {MyProjectsPage} • {Math.Min((MyProjectsPage - 1) * MyProjectsPageSize + _projects.Count, _allProjects.Count)} of {_allProjects.Count}";

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<Project> OpenProjectCommand { get; }
    public IRelayCommand<Project> CopyCloneUrlCommand { get; }
    public IRelayCommand<Project> ViewIssuesCommand { get; }
    public IRelayCommand SearchProjectsCommand { get; }
    public IRelayCommand NextPageCommand { get; }
    public IRelayCommand PreviousPageCommand { get; }
    public IRelayCommand NextMyPageCommand { get; }
    public IRelayCommand PreviousMyPageCommand { get; }

    public ProjectsViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(LoadProjectsAsync);
        OpenProjectCommand = new RelayCommand<Project>(OpenProject);
        CopyCloneUrlCommand = new RelayCommand<Project>(CopyCloneUrl);
        ViewIssuesCommand = new RelayCommand<Project>(ViewIssues);
        SearchProjectsCommand = new AsyncRelayCommand(SearchProjectsAsync);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
        NextMyPageCommand = new AsyncRelayCommand(NextMyPageAsync, () => HasNextMyPage);
        PreviousMyPageCommand = new AsyncRelayCommand(PreviousMyPageAsync, () => HasPreviousMyPage);
    }

    public void Initialize(IGitLabApiService gitLabService, INavigationService? navigationService = null)
    {
        _gitLabService = gitLabService;
        _navigationService = navigationService;
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

    private async Task SearchProjectsAsync()
    {
        if (_gitLabService == null)
        {
            LoadingMessage = "GitLab service not initialized";
            return;
        }

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            LoadingMessage = "Please enter a search term";
            Projects.Clear();
            TotalSearchResults = 0;
            CurrentPage = 1;
            return;
        }

        IsSearching = true;
        LoadingMessage = "Searching projects...";
        CurrentPage = 1; // Reset to first page on new search

        try
        {
            var results = await _gitLabService.SearchProjectsAsync(SearchQuery, CurrentPage, PageSize);
            
            Projects.Clear();
            foreach (var project in results)
            {
                Projects.Add(project);
            }

            // Estimate total based on results count and page size
            // If we got a full page, there might be more
            TotalSearchResults = results.Count == PageSize ? results.Count * 2 : results.Count;
            
            var totalPages = (int)Math.Ceiling((double)TotalSearchResults / PageSize);
            PaginationInfo = $"Page {CurrentPage} - Showing {results.Count} results";

            LoadingMessage = results.Count == 0 
                ? $"No projects found matching '{SearchQuery}'" 
                : $"Found {results.Count} projects matching '{SearchQuery}' (Page {CurrentPage})";
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Search error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task NextPageAsync()
    {
        if (IsSearching || string.IsNullOrWhiteSpace(SearchQuery))
            return;

        if (_gitLabService == null)
            return;

        IsSearching = true;
        CurrentPage++;
        LoadingMessage = "Loading next page...";

        try
        {
            var results = await _gitLabService.SearchProjectsAsync(SearchQuery, CurrentPage, PageSize);
            
            if (results.Count == 0)
            {
                CurrentPage--; // Go back if no results
                LoadingMessage = "No more pages available";
                return;
            }

            Projects.Clear();
            foreach (var project in results)
            {
                Projects.Add(project);
            }

            PaginationInfo = $"Page {CurrentPage} - Showing {results.Count} results";
            LoadingMessage = $"Page {CurrentPage}: {results.Count} projects found";
        }
        catch (Exception ex)
        {
            CurrentPage--;
            LoadingMessage = $"Error loading next page: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task PreviousPageAsync()
    {
        if (IsSearching || CurrentPage <= 1 || string.IsNullOrWhiteSpace(SearchQuery))
            return;

        if (_gitLabService == null)
            return;

        IsSearching = true;
        CurrentPage--;
        LoadingMessage = "Loading previous page...";

        try
        {
            var results = await _gitLabService.SearchProjectsAsync(SearchQuery, CurrentPage, PageSize);
            
            Projects.Clear();
            foreach (var project in results)
            {
                Projects.Add(project);
            }

            PaginationInfo = $"Page {CurrentPage} - Showing {results.Count} results";
            LoadingMessage = $"Page {CurrentPage}: {results.Count} projects found";
        }
        catch (Exception ex)
        {
            CurrentPage++;
            LoadingMessage = $"Error loading previous page: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
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

        // Reset to page 1 when filters change
        MyProjectsPage = 1;

        // Apply pagination
        var paginatedProjects = filtered
            .Skip((MyProjectsPage - 1) * MyProjectsPageSize)
            .Take(MyProjectsPageSize)
            .ToList();

        Projects.Clear();
        foreach (var project in paginatedProjects)
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

    private void ViewIssues(Project? project)
    {
        if (project == null || _navigationService == null || _gitLabService == null)
            return;

        try
        {
            var issuesViewModel = new IssuesViewModel();
            issuesViewModel.Initialize(_gitLabService, _navigationService);
            issuesViewModel.SetProject(project);
            
            _navigationService.Navigate(issuesViewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to navigate to issues: {ex.Message}");
        }
    }

    private async Task NextMyPageAsync()
    {
        if (HasNextMyPage)
        {
            MyProjectsPage++;
            ApplyFilters();
        }
    }

    private async Task PreviousMyPageAsync()
    {
        if (HasPreviousMyPage)
        {
            MyProjectsPage--;
            ApplyFilters();
        }
    }
}