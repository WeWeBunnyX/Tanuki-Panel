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
    private string _viewMode = "ProjectIssues"; // "ProjectIssues" or "SearchIssues"
    private string _searchQuery = "";
    private bool _isSearching = false;

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

    public INavigationService? NavigationService
    {
        get => _navigationService;
    }

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand<Issue> OpenIssueCommand { get; }
    public IRelayCommand SearchIssuesCommand { get; }
    public IRelayCommand BackCommand { get; }

    public IssuesViewModel()
    {
        RefreshCommand = new AsyncRelayCommand(LoadIssuesAsync);
        OpenIssueCommand = new RelayCommand<Issue>(OpenIssue);
        SearchIssuesCommand = new AsyncRelayCommand(SearchIssuesAsync);
        BackCommand = new RelayCommand(() => { });
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

        try
        {
            var issues = await _gitLabService.GetIssuesAsync(CurrentProject.Id, state: StateFilter);
            
            _allIssues = issues;
            ApplyFilters();

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

        try
        {
            var results = await _gitLabService.SearchIssuesAsync(SearchQuery);
            
            Issues.Clear();
            foreach (var issue in results)
            {
                Issues.Add(issue);
            }

            LoadingMessage = results.Count == 0 
                ? $"No issues found matching '{SearchQuery}'" 
                : $"Found {results.Count} issues matching '{SearchQuery}'";
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

        Issues.Clear();
        foreach (var issue in filtered)
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
}

