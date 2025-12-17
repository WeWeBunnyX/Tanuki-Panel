using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TanukiPanel.Models;
using TanukiPanel.Services;

namespace TanukiPanel.ViewModels;

public class CommitViewModel : ViewModelBase
{
    private ObservableCollection<Commit> _commits = new();
    private string _repositoryPath = "";
    private string _loadingMessage = "Ready";
    private bool _isLoading = false;
    private IGitLabApiService? _gitLabService;
    private Project? _selectedProject;
    private DateTime _startDate = DateTime.Now.AddMonths(-1);
    private DateTime _endDate = DateTime.Now;
    private bool _filterByDate = false;
    private int _currentPage = 1;
    private int _itemsPerPage = 20;
    private int _totalCommits = 0;

    public ObservableCollection<Commit> Commits
    {
        get => _commits;
        set => SetProperty(ref _commits, value);
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

    public DateTime StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    public bool FilterByDate
    {
        get => _filterByDate;
        set => SetProperty(ref _filterByDate, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public int ItemsPerPage
    {
        get => _itemsPerPage;
        set => SetProperty(ref _itemsPerPage, value);
    }

    public int TotalCommits
    {
        get => _totalCommits;
        set => SetProperty(ref _totalCommits, value);
    }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage * ItemsPerPage < TotalCommits;
    public string PageInfo => $"Page {CurrentPage} • Showing {Math.Min(ItemsPerPage, TotalCommits % ItemsPerPage == 0 ? ItemsPerPage : TotalCommits % ItemsPerPage)} of {TotalCommits}";

    public IRelayCommand LoadRepositoryCommand { get; }
    public IRelayCommand FetchCommitsCommand { get; }
    public IRelayCommand NextPageCommand { get; }
    public IRelayCommand PreviousPageCommand { get; }

    public CommitViewModel()
    {
        LoadRepositoryCommand = new AsyncRelayCommand(LoadRepositoryAsync);
        FetchCommitsCommand = new AsyncRelayCommand(FetchCommitsAsync);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync, () => HasNextPage);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync, () => HasPreviousPage);
    }

    public void Initialize(IGitLabApiService gitLabService)
    {
        _gitLabService = gitLabService;
        LoadingMessage = "Commit Viewer Ready";
    }

    private async Task LoadRepositoryAsync()
    {
        if (string.IsNullOrWhiteSpace(RepositoryPath))
        {
            LoadingMessage = "Please enter a repository path or URL";
            return;
        }

        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        LoadingMessage = "Loading repository...";

        try
        {
            if (_gitLabService == null)
            {
                LoadingMessage = "API service not initialized";
                return;
            }

            string projectPath = RepositoryPath.Trim();

            // Extract path from full GitLab URL if provided
            if (projectPath.StartsWith("https://") || projectPath.StartsWith("http://"))
            {
                // Parse URL: https://gitlab.com/user/project
                var uri = new Uri(projectPath);
                projectPath = uri.AbsolutePath.TrimStart('/');
            }

            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Input: {RepositoryPath}");
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Final projectPath to use: '{projectPath}'");

            var project = await _gitLabService.GetProjectByPathAsync(projectPath);

            if (project != null)
            {
                SelectedProject = project;
                CurrentPage = 1; // Reset to page 1 when loading new repo
                LoadingMessage = $"✓ Loaded: {project.Name}";
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Found project: {project.Name} (ID: {project.Id})");
                
                // Fetch commits after loading the project
                await FetchCommitsAsync();
            }
            else
            {
                LoadingMessage = "Repository not found";
                Console.WriteLine($"[ViewModel] LoadRepositoryAsync - Project not found for path: {projectPath}");
            }
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Error: {ex.Message}";
            Console.WriteLine($"[ViewModel] LoadRepositoryAsync - ERROR: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FetchCommitsAsync()
    {
        if (SelectedProject == null)
        {
            LoadingMessage = "Please load a repository first";
            return;
        }

        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        LoadingMessage = "Fetching commits...";

        try
        {
            if (_gitLabService == null)
            {
                LoadingMessage = "API service not initialized";
                return;
            }

            DateTime? since = FilterByDate ? StartDate : null;
            DateTime? until = FilterByDate ? EndDate : null;

            var commits = await _gitLabService.GetCommitsAsync(
                SelectedProject.Id,
                since,
                until,
                page: CurrentPage,
                perPage: ItemsPerPage
            );

            Commits.Clear();
            foreach (var commit in commits)
            {
                Commits.Add(commit);
            }

            // Update total count (GitLab API returns total in response headers, but we estimate)
            TotalCommits = commits.Count >= ItemsPerPage ? (CurrentPage * ItemsPerPage) + 1 : (CurrentPage - 1) * ItemsPerPage + commits.Count;
            
            LoadingMessage = $"✓ Found {commits.Count} commits on page {CurrentPage}";
            Console.WriteLine($"[ViewModel] FetchCommitsAsync - Retrieved {commits.Count} commits on page {CurrentPage}");
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Error fetching commits: {ex.Message}";
            Console.WriteLine($"[ViewModel] FetchCommitsAsync - ERROR: {ex.Message}");
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
            await FetchCommitsAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            await FetchCommitsAsync();
        }
    }}