using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
    private bool _isDownloading = false;
    private double _downloadProgress = 0;
    private string _downloadFileName = "";
    private CancellationTokenSource? _downloadCancellationToken;
    private HttpClient _httpClient = new HttpClient();

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

    public bool IsDownloading
    {
        get => _isDownloading;
        set => SetProperty(ref _isDownloading, value);
    }

    public double DownloadProgress
    {
        get => _downloadProgress;
        set => SetProperty(ref _downloadProgress, value);
    }

    public string DownloadFileName
    {
        get => _downloadFileName;
        set => SetProperty(ref _downloadFileName, value);
    }

    public IRelayCommand LoadRepositoryCommand { get; }
    public IRelayCommand<Package> DownloadPackageCommand { get; }
    public IRelayCommand CancelDownloadCommand { get; }
    public IRelayCommand RefreshCommand { get; }

    public PackageRegistryViewModel()
    {
        LoadRepositoryCommand = new AsyncRelayCommand(LoadRepositoryAsync);
        DownloadPackageCommand = new AsyncRelayCommand<Package>(DownloadPackageAsync);
        CancelDownloadCommand = new RelayCommand(CancelDownload);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public void Initialize(IGitLabApiService gitLabService)
    {
        _gitLabService = gitLabService;
        LoadingMessage = "Package Registry Ready";
    }

    private async Task DownloadPackageAsync(Package? package)
    {
        if (package == null)
        {
            LoadingMessage = "No package selected";
            return;
        }

        if (IsDownloading)
        {
            LoadingMessage = "A download is already in progress";
            return;
        }

        Console.WriteLine($"[ViewModel] DownloadPackageAsync - Downloading package: {package.Name} v{package.Version}");
        
        IsDownloading = true;
        DownloadProgress = 0;
        DownloadFileName = $"{package.Name}-{package.Version}";
        _downloadCancellationToken = new CancellationTokenSource();
        LoadingMessage = $"Downloading {package.Name} v{package.Version}...";

        try
        {
            // Create a temporary download directory
            string downloadDir = Path.Combine(Path.GetTempPath(), "TanukiPanel_Downloads");
            Directory.CreateDirectory(downloadDir);

            string fileName = $"{package.Name}_{package.Version}.zip";
            string filePath = Path.Combine(downloadDir, fileName);

            // Download the package file from GitLab
            await DownloadPackageFileAsync(filePath, package, _downloadCancellationToken.Token);

            if (!_downloadCancellationToken.Token.IsCancellationRequested)
            {
                // Get file info
                var fileInfo = new FileInfo(filePath);
                string sizeStr = FormatBytes(fileInfo.Length);
                
                LoadingMessage = $"✓ Downloaded: {fileName} ({sizeStr}) → {downloadDir}";
                Console.WriteLine($"[ViewModel] DownloadPackageAsync - Successfully downloaded to: {filePath}");
                
                // Try to open the file location
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo { UseShellExecute = true };
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        psi.FileName = "explorer.exe";
                        psi.Arguments = $"/select, \"{filePath}\"";
                    }
                    else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                    {
                        psi.FileName = "open";
                        psi.Arguments = $"-R \"{filePath}\"";
                    }
                    else
                    {
                        psi.FileName = "xdg-open";
                        psi.Arguments = downloadDir;
                    }
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ViewModel] DownloadPackageAsync - Could not open file location: {ex.Message}");
                }
            }
            else
            {
                LoadingMessage = "Download cancelled";
                Console.WriteLine($"[ViewModel] DownloadPackageAsync - Download was cancelled");
                // Clean up partially downloaded file
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }
            }
        }
        catch (OperationCanceledException)
        {
            LoadingMessage = "Download cancelled";
            Console.WriteLine($"[ViewModel] DownloadPackageAsync - Download cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] DownloadPackageAsync - ERROR: {ex.Message}");
            LoadingMessage = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
            _downloadCancellationToken?.Dispose();
            _downloadCancellationToken = null;
        }
    }

    private async Task DownloadPackageFileAsync(string filePath, Package package, CancellationToken cancellationToken)
    {
        try
        {
            if (SelectedProject == null)
            {
                throw new InvalidOperationException("No project selected");
            }

            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Starting download for package: {package.Name} (ID: {package.Id})");
            
            // Get package files list first to get the file ID
            string filesListUrl = $"https://gitlab.com/api/v4/projects/{SelectedProject.Id}/packages/{package.Id}/package_files";
            
            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Fetching package files from: {filesListUrl}");
            
            var packageFilesResponse = await _httpClient.GetAsync(filesListUrl);
            if (!packageFilesResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Could not get package files list: {packageFilesResponse.StatusCode}");
                throw new Exception($"Failed to get package files: {packageFilesResponse.StatusCode}");
            }

            var filesJson = await packageFilesResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Package files response: {filesJson.Substring(0, Math.Min(200, filesJson.Length))}");

            // Parse JSON to get first file ID
            using (var jsonDoc = System.Text.Json.JsonDocument.Parse(filesJson))
            {
                var fileArray = jsonDoc.RootElement;
                if (fileArray.GetArrayLength() == 0)
                {
                    throw new Exception("No files found in package");
                }
                
                var firstFile = fileArray[0];
                int fileId = firstFile.GetProperty("id").GetInt32();
                string fileName = firstFile.GetProperty("file_name").GetString() ?? "unknown";
                long fileSize = firstFile.GetProperty("size").GetInt64();
                
                Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Found file: {fileName} (ID: {fileId}, Size: {FormatBytes(fileSize)})");

                // Use the correct download endpoint for package files
                string fileDownloadUrl = $"https://gitlab.com/api/v4/projects/{SelectedProject.Id}/packages/{package.Id}/package_files/{fileId}/download";
                
                Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Attempting download from: {fileDownloadUrl}");
                
                using (var downloadResponse = await _httpClient.GetAsync(fileDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (!downloadResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Download failed: {downloadResponse.StatusCode}");
                        var errorBody = await downloadResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Error response: {errorBody}");
                        throw new Exception($"Download failed: {downloadResponse.StatusCode}");
                    }

                    long? contentLength = downloadResponse.Content.Headers.ContentLength;
                    Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - File size from response: {(contentLength.HasValue ? FormatBytes(contentLength.Value) : "Unknown")}");

                    using (var contentStream = await downloadResponse.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            totalBytesRead += bytesRead;

                            if (contentLength.HasValue)
                            {
                                DownloadProgress = (double)totalBytesRead / contentLength.Value * 100;
                                LoadingMessage = $"Downloading {package.Name}... {(int)DownloadProgress}% ({FormatBytes(totalBytesRead)}/{FormatBytes(contentLength.Value)})";
                            }
                            else
                            {
                                LoadingMessage = $"Downloading {package.Name}... {FormatBytes(totalBytesRead)} downloaded";
                            }

                            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Downloaded {FormatBytes(totalBytesRead)}");
                        }
                    }
                }
            }

            DownloadProgress = 100;
            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Download completed successfully");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Download cancelled by user");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - ERROR during download: {ex.Message}");
            Console.WriteLine($"[ViewModel] DownloadPackageFileAsync - Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void CancelDownload()
    {
        if (_downloadCancellationToken != null && !_downloadCancellationToken.Token.IsCancellationRequested)
        {
            Console.WriteLine($"[ViewModel] CancelDownload - Cancelling download");
            _downloadCancellationToken.Cancel();
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    // ...existing code...

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

    private async Task RefreshAsync()
    {
        Console.WriteLine($"[ViewModel] RefreshAsync - Refreshing package list");
        await LoadRepositoryAsync();
    }
}
