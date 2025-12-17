using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class IssuesViewModel : ViewModelBase
{
    private ObservableCollection<Issue> _issues = new();
    private List<Issue> _allIssues = new();
    private string _loadingMessage = "Loading issues...";
    private bool _isLoading = true;
    private string _searchText = "";
    private string _stateFilter = "all"; // all, opened, closed
    private string _sortBy = "UpdatedAt"; // UpdatedAt, CreatedAt, Title
    private IGitLabApiService? _gitLabService;
    private INavigationService? _navigationService;
    private Project? _currentProject;
    private Project? _searchedProject;
    private string _viewMode = "ProjectIssues"; // "ProjectIssues", "SearchIssues", or "SearchRepository"
    private string _searchQuery = "";
    private string _repositoryPath = "";
    private bool _isSearching = false;
    private int _currentPage = 1;
    private const int IssuesPerPage = 15;
    private bool _isCreatingIssue = false;
    private string _newIssueTitle = "";
    private string _newIssueDescription = "";

    public ObservableCollection<Issue> Issues
    {
        get => _issues;
        set => SetProperty(ref _issues, value);
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

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                if (_viewMode == "ProjectIssues")
                {
                    ApplyFilters();
                }
            }
        }
    }

    public string StateFilter
    {
        get => _stateFilter;
        set
        {
            if (SetProperty(ref _stateFilter, value))
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
                if (value == "ProjectIssues")
                {
                    ApplyFilters();
                }
                else
                {
                    Issues.Clear();
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

    public Project? CurrentProject
    {
        get => _currentProject;
        set => SetProperty(ref _currentProject, value);
    }

    public string RepositoryPath
    {
        get => _repositoryPath;
        set => SetProperty(ref _repositoryPath, value);
    }

    public INavigationService? NavigationService
    {
        get => _navigationService;
    }

    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage * IssuesPerPage < _allIssues.Count;
    public string IssuesPageInfo => $"Page {CurrentPage} • {Math.Min(IssuesPerPage, _allIssues.Count % IssuesPerPage == 0 ? IssuesPerPage : _allIssues.Count % IssuesPerPage)} of {_allIssues.Count}";

    public bool IsCreatingIssue
    {
        get => _isCreatingIssue;
        set => SetProperty(ref _isCreatingIssue, value);
    }

    public string NewIssueTitle
    {
        get => _newIssueTitle;
        set => SetProperty(ref _newIssueTitle, value);
    }

    public string NewIssueDescription
    {
        get => _newIssueDescription;
        set => SetProperty(ref _newIssueDescription, value);
    }

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<Issue> OpenIssueCommand { get; }
    public IRelayCommand SearchIssuesCommand { get; }
    public IRelayCommand LoadRepositoryIssuesCommand { get; }
    public IRelayCommand<Issue> ToggleIssueStateCommand { get; }
    public IRelayCommand BackCommand { get; }
    public IRelayCommand NextPageCommand { get; }
    public IRelayCommand PreviousPageCommand { get; }
    public IRelayCommand ShowCreateIssueDialogCommand { get; }
    public IRelayCommand CreateIssueCommand { get; }
    public IRelayCommand CancelCreateIssueCommand { get; }

    public IssuesViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(LoadIssuesAsync);
        OpenIssueCommand = new RelayCommand<Issue>(OpenIssue);
        SearchIssuesCommand = new AsyncRelayCommand(SearchIssuesAsync);
        LoadRepositoryIssuesCommand = new AsyncRelayCommand(LoadRepositoryIssuesAsync);
        ToggleIssueStateCommand = new AsyncRelayCommand<Issue>(ToggleIssueStateAsync);
        BackCommand = new RelayCommand(() => { });
        NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => HasNextPage);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => HasPreviousPage);
        ShowCreateIssueDialogCommand = new RelayCommand(ShowCreateIssueDialog);
        CreateIssueCommand = new AsyncRelayCommand(CreateNewIssueAsync);
        CancelCreateIssueCommand = new RelayCommand(CancelCreateIssue);
    }

    public void Initialize(IGitLabApiService gitLabService, INavigationService? navigationService = null)
    {
        _gitLabService = gitLabService;
        _navigationService = navigationService;
        // Store navigation service if needed for back button
    }

    public void SetProject(Project project)
    {
        CurrentProject = project;
        _allIssues.Clear();
        Issues.Clear();
        _ = LoadIssuesAsync();
    }

    private async Task LoadIssuesAsync()
    {
        if (_gitLabService == null || CurrentProject == null)
        {
            LoadingMessage = "GitLab service or project not initialized";
            IsLoading = false;
            return;
        }

        IsLoading = true;
        LoadingMessage = $"Fetching issues for {CurrentProject.Name}...";
        Console.WriteLine($"[ViewModel] LoadIssuesAsync - Loading issues for project '{CurrentProject.Name}' (ID: {CurrentProject.Id}), state filter: {StateFilter}");

        try
        {
            var issues = await _gitLabService.GetIssuesAsync(CurrentProject.Id, state: StateFilter);
            
            _allIssues = issues;
            ApplyFilters();
            Console.WriteLine($"[ViewModel] LoadIssuesAsync - Received {issues.Count} issues, displaying {Issues.Count} after filters");

            if (Issues.Count == 0)
            {
                LoadingMessage = $"No {StateFilter} issues found";
            }
            else
            {
                LoadingMessage = $"Loaded {Issues.Count} of {_allIssues.Count} issues";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] LoadIssuesAsync - ERROR: {ex.Message}\nStackTrace: {ex.StackTrace}");
            LoadingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchIssuesAsync()
    {
        if (_gitLabService == null)
        {
            LoadingMessage = "GitLab service not initialized";
            return;
        }

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            LoadingMessage = "Please enter a search term";
            Issues.Clear();
            return;
        }

        IsSearching = true;
        LoadingMessage = "Searching issues...";
        Console.WriteLine($"[ViewModel] SearchIssuesAsync - Searching for issues with query: '{SearchQuery}'");

        try
        {
            var results = await _gitLabService.SearchIssuesAsync(SearchQuery);
            
            Issues.Clear();
            foreach (var issue in results)
            {
                Issues.Add(issue);
            }
            Console.WriteLine($"[ViewModel] SearchIssuesAsync - Found {results.Count} issues");

            LoadingMessage = results.Count == 0 
                ? $"No issues found matching '{SearchQuery}'" 
                : $"Found {results.Count} issues matching '{SearchQuery}'";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] SearchIssuesAsync - ERROR: {ex.Message}\nStackTrace: {ex.StackTrace}");
            LoadingMessage = $"Search error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allIssues.AsEnumerable();

        // Search by title or description
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(i => 
                i.Title.ToLower().Contains(searchLower) || 
                i.Description.ToLower().Contains(searchLower));
        }

        // Sort
        filtered = SortBy switch
        {
            "Title" => filtered.OrderBy(i => i.Title),
            "CreatedAt" => filtered.OrderByDescending(i => i.CreatedAt),
            _ => filtered.OrderByDescending(i => i.UpdatedAt)
        };

        // Reset to page 1 when filters change
        CurrentPage = 1;

        // Apply pagination
        var paginatedIssues = filtered
            .Skip((CurrentPage - 1) * IssuesPerPage)
            .Take(IssuesPerPage)
            .ToList();

        Issues.Clear();
        foreach (var issue in paginatedIssues)
        {
            Issues.Add(issue);
        }
    }

    private void OpenIssue(Issue? issue)
    {
        if (issue == null) return;
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo { UseShellExecute = true };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                psi.FileName = issue.WebUrl;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                psi.FileName = "open";
                psi.Arguments = issue.WebUrl;
            }
            else
            {
                psi.FileName = "xdg-open";
                psi.Arguments = issue.WebUrl;
            }
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open issue: {ex.Message}");
        }
    }

    private async Task LoadRepositoryIssuesAsync()
    {
        if (_gitLabService == null || string.IsNullOrWhiteSpace(RepositoryPath))
        {
            LoadingMessage = "Please enter a repository path or URL";
            return;
        }

        IsLoading = true;
        LoadingMessage = "Searching for repository...";
        Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - Loading repository from input: {RepositoryPath}");

        try
        {
            // Extract project path from URL if needed
            var projectPath = RepositoryPath.Trim();
            if (projectPath.Contains("http"))
            {
                // Extract path from URL like "https://gitlab.com/group/project"
                Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - Detected URL, parsing...");
                var uri = new Uri(projectPath);
                projectPath = uri.AbsolutePath.TrimStart('/').TrimEnd('/');
                if (projectPath.EndsWith(".git"))
                    projectPath = projectPath.Substring(0, projectPath.Length - 4);
                Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - Extracted project path: {projectPath}");
            }

            var project = await _gitLabService.GetProjectByPathAsync(projectPath);
            if (project == null)
            {
                LoadingMessage = $"Repository '{projectPath}' not found";
                Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - Project not found: {projectPath}");
                Issues.Clear();
                return;
            }

            _searchedProject = project;
            CurrentProject = project; // Set CurrentProject so "New Issue" works
            LoadingMessage = $"Fetching issues for {project.Name}...";
            Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - Found project: {project.Name} (ID: {project.Id}), loading issues...");

            var issues = await _gitLabService.GetIssuesAsync(project.Id, state: StateFilter);
            _allIssues = issues;
            ApplyFilters();
            Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - Loaded {issues.Count} issues, displaying {Issues.Count} after filters");

            LoadingMessage = issues.Count == 0
                ? $"No {StateFilter} issues found in {project.Name}"
                : $"Loaded {Issues.Count} of {_allIssues.Count} issues from {project.Name}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] LoadRepositoryIssuesAsync - ERROR: {ex.Message}\nStackTrace: {ex.StackTrace}");
            LoadingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleIssueStateAsync(Issue? issue)
    {
        if (issue == null || _gitLabService == null)
            return;

        var projectId = CurrentProject?.Id ?? _searchedProject?.Id;
        if (projectId == null)
            return;

        var newState = issue.State == "opened" ? "close" : "reopen";
        IsLoading = true;
        Console.WriteLine($"[ViewModel] ToggleIssueStateAsync - Toggling issue #{issue.Iid} ('{issue.Title}') from '{issue.State}' to '{newState}'");

        try
        {
            var success = await _gitLabService.UpdateIssueStateAsync(projectId.Value, issue.Iid, newState);
            if (success)
            {
                LoadingMessage = $"Issue {newState}d successfully";
                Console.WriteLine($"[ViewModel] ToggleIssueStateAsync - Successfully toggled issue, reloading...");
                // Reload issues to update state
                if (ViewMode == "ProjectIssues" && CurrentProject != null)
                    _ = LoadIssuesAsync();
                else if (ViewMode == "SearchRepository")
                    _ = LoadRepositoryIssuesAsync();
            }
            else
            {
                Console.WriteLine($"[ViewModel] ToggleIssueStateAsync - Failed to toggle issue state");
                LoadingMessage = "Failed to update issue state";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] ToggleIssueStateAsync - ERROR: {ex.Message}\nStackTrace: {ex.StackTrace}");
            LoadingMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NextPageAsync()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            ApplyFilters();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            ApplyFilters();
        }
    }

    private void ShowCreateIssueDialog()
    {
        Console.WriteLine("[ViewModel] ShowCreateIssueDialog - Opening create issue dialog");
        IsCreatingIssue = true;
        NewIssueTitle = "";
        NewIssueDescription = "";
    }

    private void CancelCreateIssue()
    {
        Console.WriteLine("[ViewModel] CancelCreateIssue - Closing create issue dialog");
        IsCreatingIssue = false;
        NewIssueTitle = "";
        NewIssueDescription = "";
    }

    private async Task CreateNewIssueAsync()
    {
        Console.WriteLine("[ViewModel] CreateNewIssueAsync - Starting create issue process");
        Console.WriteLine($"[ViewModel] CreateNewIssueAsync - Title: '{NewIssueTitle}', Description: '{NewIssueDescription}'");
        Console.WriteLine($"[ViewModel] CreateNewIssueAsync - CurrentProject: {CurrentProject?.Name ?? "NULL"}, GitLabService: {(_gitLabService == null ? "NULL" : "OK")}");
        
        if (string.IsNullOrWhiteSpace(NewIssueTitle))
        {
            Console.WriteLine("[ViewModel] CreateNewIssueAsync - Title is empty, aborting");
            LoadingMessage = "Please enter an issue title";
            return;
        }

        if (_gitLabService == null)
        {
            Console.WriteLine("[ViewModel] CreateNewIssueAsync - GitLabService is null, aborting");
            LoadingMessage = "GitLab service not initialized";
            return;
        }

        if (CurrentProject == null)
        {
            Console.WriteLine("[ViewModel] CreateNewIssueAsync - CurrentProject is null, aborting");
            LoadingMessage = "No project selected. Please select a project first.";
            return;
        }

        IsLoading = true;
        LoadingMessage = "Creating issue...";
        Console.WriteLine($"[ViewModel] CreateNewIssueAsync - Creating issue '{NewIssueTitle}' in project {CurrentProject.Name}");

        try
        {
            var newIssue = await _gitLabService.CreateIssueAsync(CurrentProject.Id, NewIssueTitle, NewIssueDescription);
            
            if (newIssue != null)
            {
                Console.WriteLine($"[ViewModel] CreateNewIssueAsync - Successfully created issue #{newIssue.Iid}");
                LoadingMessage = $"Issue created successfully: #{newIssue.Iid}";
                
                // Clear form and close dialog
                IsCreatingIssue = false;
                NewIssueTitle = "";
                NewIssueDescription = "";
                
                // Reload issues
                CurrentPage = 1;
                await LoadIssuesAsync();
            }
            else
            {
                LoadingMessage = "Failed to create issue";
                Console.WriteLine($"[ViewModel] CreateNewIssueAsync - Failed to create issue");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] CreateNewIssueAsync - ERROR: {ex.Message}\nStackTrace: {ex.StackTrace}");
            LoadingMessage = $"Error creating issue: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
