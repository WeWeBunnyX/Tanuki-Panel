using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TanukiPanel.Models;

namespace TanukiPanel.Services;

/// <summary>
/// Service for interacting with GitLab API
/// </summary>
public interface IGitLabApiService
{
    /// <summary>
    /// Fetches all accessible projects for the authenticated user
    /// </summary>
    Task<List<Project>> GetProjectsAsync(int page = 1, int perPage = 20);

    /// <summary>
    /// Searches for projects by name across all accessible projects
    /// </summary>
    Task<List<Project>> SearchProjectsAsync(string query, int page = 1, int perPage = 20);

    /// <summary>
    /// Fetches issues for a specific project
    /// </summary>
    Task<List<Issue>> GetIssuesAsync(int projectId, int page = 1, int perPage = 20, string state = "all");

    /// <summary>
    /// Searches for issues across all projects or within a specific project
    /// </summary>
    Task<List<Issue>> SearchIssuesAsync(string query, int page = 1, int perPage = 20);

    /// <summary>
    /// Gets a project by its path (e.g., "group/project")
    /// </summary>
    Task<Project?> GetProjectByPathAsync(string projectPath);

    /// <summary>
    /// Updates an issue state (open/close)
    /// </summary>
    Task<bool> UpdateIssueStateAsync(int projectId, int issueIid, string newState);

    /// <summary>
    /// Tests the API connection and authentication
    /// </summary>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Gets the current authenticated user info
    /// </summary>
    Task<string> GetCurrentUserAsync();

    /// <summary>
    /// Gets the current authenticated user with full details including avatar
    /// </summary>
    Task<User?> GetCurrentUserDetailedAsync();

    /// <summary>
    /// Fetches container registry repositories for a specific project
    /// </summary>
    Task<List<RegistryRepository>> GetRegistryRepositoriesAsync(int projectId, int page = 1, int perPage = 20);

    /// <summary>
    /// Fetches tags for a specific registry repository
    /// </summary>
    Task<List<RegistryTag>> GetRegistryTagsAsync(int projectId, int repositoryId, int page = 1, int perPage = 20);

    /// <summary>
    /// Fetches packages from the Package Registry for a specific project
    /// </summary>
    Task<List<Package>> GetPackagesAsync(int projectId, int page = 1, int perPage = 20);

    /// <summary>
    /// Deletes a tag from the container registry
    /// </summary>
    Task<bool> DeleteRegistryTagAsync(int projectId, int repositoryId, string tagName);

    /// <summary>
    /// Fetches the logs for a specific registry tag (if available)
    /// </summary>
    Task<string> GetRegistryTagLogsAsync(int projectId, int repositoryId, string tagName);

    /// <summary>
    /// Uploads a file to the Container Registry as a blob
    /// </summary>
    Task<bool> UploadContainerImageAsync(int projectId, string filePath, string fileName, IProgress<(long BytesRead, long TotalBytes)>? progress = null);

    /// <summary>
    /// Uploads a file to the Package Registry
    /// </summary>
    Task<bool> UploadPackageFileAsync(int projectId, string filePath, string packageName, string packageVersion, string packageType = "generic", IProgress<(long BytesRead, long TotalBytes)>? progress = null);

    /// <summary>
    /// Fetches commits for a specific project with optional date range filtering
    /// </summary>
    Task<List<Commit>> GetCommitsAsync(int projectId, DateTime? since = null, DateTime? until = null, int page = 1, int perPage = 20);
}

public class GitLabApiService : IGitLabApiService
{
    private readonly string _apiToken;
    private readonly string _gitlabUrl;
    private readonly HttpClient _httpClient;

    public GitLabApiService(string gitlabUrl, string apiToken)
    {
        _gitlabUrl = gitlabUrl.TrimEnd('/');
        _apiToken = apiToken;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", _apiToken);
        Console.WriteLine($"[API] GitLabApiService initialized - URL: {_gitlabUrl}, Token length: {_apiToken?.Length ?? 0}");
    }

    public async Task<List<Project>> GetProjectsAsync(int page = 1, int perPage = 20)
    {
        try
        {
            // membership=true gets only projects where the user is a member
            var url = $"{_gitlabUrl}/api/v4/projects?page={page}&per_page={perPage}&membership=true&simple=false&order_by=last_activity_at&sort=desc";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GitLab API Error: {response.StatusCode}");
                return new List<Project>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var projects = JsonSerializer.Deserialize<List<Project>>(json, options) ?? new List<Project>();
            return projects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch projects: {ex.Message}");
            return new List<Project>();
        }
    }

    public async Task<List<Project>> SearchProjectsAsync(string query, int page = 1, int perPage = 20)
    {
        try
        {
            // Search across all accessible projects by name
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{_gitlabUrl}/api/v4/projects?search={encodedQuery}&page={page}&per_page={perPage}&simple=false&order_by=last_activity_at&sort=desc";
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"GitLab API Error: {response.StatusCode}");
                return new List<Project>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var projects = JsonSerializer.Deserialize<List<Project>>(json, options) ?? new List<Project>();
            return projects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to search projects: {ex.Message}");
            return new List<Project>();
        }
    }

    public async Task<List<Issue>> GetIssuesAsync(int projectId, int page = 1, int perPage = 20, string state = "all")
    {
        try
        {
            // Fetch issues for a specific project
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/issues?page={page}&per_page={perPage}&state={state}&order_by=updated_at&sort=desc";
            
            Console.WriteLine($"[API] GetIssuesAsync - Fetching issues for project {projectId}, state={state}, page={page}");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: GitLab API Error: {response.StatusCode}");
                return new List<Issue>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var issues = JsonSerializer.Deserialize<List<Issue>>(json, options) ?? new List<Issue>();
            Console.WriteLine($"[API] Successfully fetched {issues.Count} issues");
            return issues;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to fetch issues: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return new List<Issue>();
        }
    }

    public async Task<List<Issue>> SearchIssuesAsync(string query, int page = 1, int perPage = 20)
    {
        try
        {
            // Search issues across all projects
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{_gitlabUrl}/api/v4/issues?search={encodedQuery}&page={page}&per_page={perPage}&order_by=updated_at&sort=desc";
            
            Console.WriteLine($"[API] SearchIssuesAsync - Searching for issues with query: {query}, page={page}");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: Search failed (Status: {response.StatusCode})");
                return new List<Issue>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var issues = JsonSerializer.Deserialize<List<Issue>>(json, options) ?? new List<Issue>();
            Console.WriteLine($"[API] Successfully found {issues.Count} issues");
            return issues;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to search issues: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return new List<Issue>();
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/user";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetCurrentUserAsync()
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/user";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return "Unknown";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("name", out var nameElement))
            {
                return nameElement.GetString() ?? "Unknown";
            }

            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    public async Task<User?> GetCurrentUserDetailedAsync()
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/user";
            Console.WriteLine($"[API] GetCurrentUserDetailedAsync - Fetching current user details");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: Failed to fetch user details (Status: {response.StatusCode})");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var user = JsonSerializer.Deserialize<User>(json, options);
            if (user != null)
            {
                Console.WriteLine($"[API] Successfully fetched user: {user.Name} (@{user.Username})");
                Console.WriteLine($"[API] Avatar URL: {user.AvatarUrl}");
            }
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to fetch user details: {ex.Message}");
            return null;
        }
    }

    public async Task<Project?> GetProjectByPathAsync(string projectPath)
    {
        try
        {
            var encodedPath = Uri.EscapeDataString(projectPath);
            var url = $"{_gitlabUrl}/api/v4/projects/{encodedPath}";
            
            Console.WriteLine($"[API] GetProjectByPathAsync - Searching for project: {projectPath}");
            Console.WriteLine($"[API] URL: {url}");
            Console.WriteLine($"[API] Authorization header present: {_httpClient.DefaultRequestHeaders.Contains("PRIVATE-TOKEN")}");
            Console.WriteLine($"[API] Token: {(_apiToken?.Substring(0, Math.Min(10, _apiToken.Length)) + "..." ?? "NONE")}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: Project not found (Status: {response.StatusCode})");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Response body: {errorContent}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var project = JsonSerializer.Deserialize<Project>(json, options);
            if (project != null)
            {
                Console.WriteLine($"[API] Successfully found project: {project.Name} (ID: {project.Id})");
            }
            return project;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to get project: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<bool> UpdateIssueStateAsync(int projectId, int issueIid, string newState)
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/issues/{issueIid}";
            
            Console.WriteLine($"[API] UpdateIssueStateAsync - Updating issue #{issueIid} in project {projectId} to state: {newState}");
            Console.WriteLine($"[API] URL: {url}");
            
            var content = new StringContent(
                JsonSerializer.Serialize(new { state_event = newState }),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            
            var response = await _httpClient.PutAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] Successfully updated issue #{issueIid} to {newState}");
                return true;
            }
            else
            {
                Console.WriteLine($"[API] ERROR: Failed to update issue (Status: {response.StatusCode})");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to update issue: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    // ...existing code...


    public async Task<List<RegistryRepository>> GetRegistryRepositoriesAsync(int projectId, int page = 1, int perPage = 20)
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/registry/repositories?page={page}&per_page={perPage}";
            
            Console.WriteLine($"[API] GetRegistryRepositoriesAsync - Fetching registries for project {projectId}");
            Console.WriteLine($"[API] URL: {url}");
            Console.WriteLine($"[API] GitLab Base URL: {_gitlabUrl}");
            
            var response = await _httpClient.GetAsync(url);
            Console.WriteLine($"[API] GetRegistryRepositoriesAsync - Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] ERROR: GitLab API Error: {response.StatusCode}");
                Console.WriteLine($"[API] Error response body: {errorContent}");
                return new List<RegistryRepository>();
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[API] GetRegistryRepositoriesAsync - Raw JSON response length: {json.Length}");
            Console.WriteLine($"[API] GetRegistryRepositoriesAsync - Raw JSON: {(json.Length > 200 ? json.Substring(0, 200) + "..." : json)}");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var registries = JsonSerializer.Deserialize<List<RegistryRepository>>(json, options) ?? new List<RegistryRepository>();
            Console.WriteLine($"[API] Successfully deserialized {registries.Count} registries");
            foreach (var registry in registries)
            {
                Console.WriteLine($"[API]   - Registry: {registry.Name} (ID: {registry.Id}, Path: {registry.Path})");
            }
            return registries;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to fetch registries: {ex.Message}");
            Console.WriteLine($"[API] Exception type: {ex.GetType().Name}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return new List<RegistryRepository>();
        }
    }

    public async Task<List<RegistryTag>> GetRegistryTagsAsync(int projectId, int repositoryId, int page = 1, int perPage = 20)
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/registry/repositories/{repositoryId}/tags?page={page}&per_page={perPage}";
            
            Console.WriteLine($"[API] GetRegistryTagsAsync - Fetching tags for registry {repositoryId} in project {projectId}");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: GitLab API Error: {response.StatusCode}");
                return new List<RegistryTag>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var tags = JsonSerializer.Deserialize<List<RegistryTag>>(json, options) ?? new List<RegistryTag>();
            Console.WriteLine($"[API] Successfully fetched {tags.Count} tags");
            return tags;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to fetch tags: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return new List<RegistryTag>();
        }
    }

    public async Task<bool> DeleteRegistryTagAsync(int projectId, int repositoryId, string tagName)
    {
        try
        {
            var encodedTag = Uri.EscapeDataString(tagName);
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/registry/repositories/{repositoryId}/tags/{encodedTag}";
            
            Console.WriteLine($"[API] DeleteRegistryTagAsync - Deleting tag '{tagName}' from registry {repositoryId}");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.DeleteAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] Successfully deleted tag '{tagName}'");
                return true;
            }
            else
            {
                Console.WriteLine($"[API] ERROR: Failed to delete tag (Status: {response.StatusCode})");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to delete tag: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<string> GetRegistryTagLogsAsync(int projectId, int repositoryId, string tagName)
    {
        try
        {
            // GitLab doesn't have a dedicated registry tag logs endpoint
            // We'll return a formatted string with available information about the tag
            var encodedTag = Uri.EscapeDataString(tagName);
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/registry/repositories/{repositoryId}/tags/{encodedTag}";
            
            Console.WriteLine($"[API] GetRegistryTagLogsAsync - Fetching details for tag '{tagName}'");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: Failed to fetch tag details (Status: {response.StatusCode})");
                return $"Error: Could not fetch logs for tag '{tagName}'";
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var tag = JsonSerializer.Deserialize<RegistryTag>(json, options);
            if (tag != null)
            {
                var logs = $@"Container Registry Tag Information
========================================
Tag Name: {tag.Name}
Digest: {tag.Digest}
Revision: {tag.ShortRevision}
Created: {tag.CreatedAt:yyyy-MM-dd HH:mm:ss}
Total Size: {FormatBytes(tag.TotalSize)}
Location: {tag.Location}
========================================

This tag represents a container image in the registry.
Use 'docker pull {tag.Location}' to pull this image.";
                
                return logs;
            }

            return $"No information available for tag '{tagName}'";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to fetch tag logs: {ex.Message}");
            return $"Error fetching logs: {ex.Message}";
        }
    }

    public async Task<List<Package>> GetPackagesAsync(int projectId, int page = 1, int perPage = 20)
    {
        try
        {
            var url = $"{_gitlabUrl}/api/v4/projects/{projectId}/packages?page={page}&per_page={perPage}";
            
            Console.WriteLine($"[API] GetPackagesAsync - Fetching packages for project {projectId}");
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            Console.WriteLine($"[API] GetPackagesAsync - Response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] ERROR: GitLab API Error: {response.StatusCode}");
                Console.WriteLine($"[API] Error response body: {errorContent}");
                return new List<Package>();
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[API] GetPackagesAsync - Raw JSON response length: {json.Length}");
            Console.WriteLine($"[API] GetPackagesAsync - Raw JSON: {(json.Length > 200 ? json.Substring(0, 200) + "..." : json)}");
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var packages = JsonSerializer.Deserialize<List<Package>>(json, options) ?? new List<Package>();
            Console.WriteLine($"[API] Successfully deserialized {packages.Count} packages");
            foreach (var package in packages)
            {
                Console.WriteLine($"[API]   - Package: {package.Name} v{package.Version} ({package.PackageType})");
            }
            return packages;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] ERROR - Failed to fetch packages: {ex.Message}");
            Console.WriteLine($"[API] Exception type: {ex.GetType().Name}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return new List<Package>();
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

    public async Task<bool> UploadContainerImageAsync(int projectId, string filePath, string fileName, IProgress<(long BytesRead, long TotalBytes)>? progress = null)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"[API] UploadContainerImageAsync - File not found: {filePath}");
                return false;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            Console.WriteLine($"[API] UploadContainerImageAsync - Uploading container image");
            Console.WriteLine($"[API] File: {fileName} ({FormatBytes(fileInfo.Length)})");
            Console.WriteLine($"[API] Project ID: {projectId}");

            // Try different GitLab endpoints that might accept file uploads
            // First, try the generic file upload endpoint
            string uploadUrl = $"{_gitlabUrl}/api/v4/projects/{projectId}/repository/files/{Uri.EscapeDataString("container-" + fileName)}";
            
            Console.WriteLine($"[API] Upload URL: {uploadUrl}");

            using (var fileStream = System.IO.File.OpenRead(filePath))
            using (var progressStream = new ProgressStream(fileStream, fileInfo.Length, progress))
            using (var content = new StreamContent(progressStream))
            {
                // Use raw binary upload for generic file endpoint
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                
                // Prepare the request with base64 encoded content
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var base64Content = Convert.ToBase64String(fileBytes);
                
                var requestBody = new
                {
                    branch = "main",
                    content = base64Content,
                    commit_message = $"Add container image: {fileName}"
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = jsonContent
                };

                Console.WriteLine($"[API] UploadContainerImageAsync - Sending request to repository files endpoint...");
                var response = await _httpClient.SendAsync(request);

                Console.WriteLine($"[API] UploadContainerImageAsync - Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine($"[API] UploadContainerImageAsync - Upload completed successfully");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Try Package Registry upload as fallback
                    Console.WriteLine($"[API] UploadContainerImageAsync - Repository files endpoint not found, trying package registry...");
                    return await UploadAsPackageAsync(projectId, filePath, fileName, progress);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] UploadContainerImageAsync - Upload failed: {response.StatusCode}");
                    Console.WriteLine($"[API] Error response: {errorContent}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] UploadContainerImageAsync - ERROR: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    private async Task<bool> UploadAsPackageAsync(int projectId, string filePath, string fileName, IProgress<(long BytesRead, long TotalBytes)>? progress = null)
    {
        try
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            
            // Use Package Registry API - this is more reliable for file uploads
            string uploadUrl = $"{_gitlabUrl}/api/v4/projects/{projectId}/packages/generic/container-images/latest/{Uri.EscapeDataString(fileName)}";
            
            Console.WriteLine($"[API] UploadAsPackageAsync - Using Package Registry endpoint");
            Console.WriteLine($"[API] Upload URL: {uploadUrl}");

            using (var fileStream = System.IO.File.OpenRead(filePath))
            using (var progressStream = new ProgressStream(fileStream, fileInfo.Length, progress))
            using (var content = new StreamContent(progressStream))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                
                var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = content
                };

                Console.WriteLine($"[API] UploadAsPackageAsync - Sending PUT request...");
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                Console.WriteLine($"[API] UploadAsPackageAsync - Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine($"[API] UploadAsPackageAsync - Upload completed successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] UploadAsPackageAsync - Upload failed: {response.StatusCode}");
                    Console.WriteLine($"[API] Error response: {errorContent}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] UploadAsPackageAsync - ERROR: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> UploadPackageFileAsync(int projectId, string filePath, string packageName, string packageVersion, string packageType = "generic", IProgress<(long BytesRead, long TotalBytes)>? progress = null)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"[API] UploadPackageFileAsync - File not found: {filePath}");
                return false;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            Console.WriteLine($"[API] UploadPackageFileAsync - Uploading package file");
            Console.WriteLine($"[API] File: {System.IO.Path.GetFileName(filePath)} ({FormatBytes(fileInfo.Length)})");
            Console.WriteLine($"[API] Project ID: {projectId}");
            Console.WriteLine($"[API] Package: {packageName} v{packageVersion} (Type: {packageType})");

            // Use Package Registry API with package name and version
            string uploadUrl = $"{_gitlabUrl}/api/v4/projects/{projectId}/packages/generic/{Uri.EscapeDataString(packageName)}/{Uri.EscapeDataString(packageVersion)}/{Uri.EscapeDataString(System.IO.Path.GetFileName(filePath))}";
            
            Console.WriteLine($"[API] Upload URL: {uploadUrl}");

            using (var fileStream = System.IO.File.OpenRead(filePath))
            using (var progressStream = new ProgressStream(fileStream, fileInfo.Length, progress))
            using (var content = new StreamContent(progressStream))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                
                var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = content
                };

                Console.WriteLine($"[API] UploadPackageFileAsync - Sending PUT request...");
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                Console.WriteLine($"[API] UploadPackageFileAsync - Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine($"[API] UploadPackageFileAsync - Upload completed successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] UploadPackageFileAsync - Upload failed: {response.StatusCode}");
                    Console.WriteLine($"[API] Error response: {errorContent}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] UploadPackageFileAsync - ERROR: {ex.Message}");
            Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<List<Commit>> GetCommitsAsync(int projectId, DateTime? since = null, DateTime? until = null, int page = 1, int perPage = 20)
    {
        try
        {
            Console.WriteLine($"[API] GetCommitsAsync - Fetching commits for project {projectId}");
            
            string url = $"{_gitlabUrl}/api/v4/projects/{projectId}/repository/commits?page={page}&per_page={perPage}";
            
            if (since.HasValue)
            {
                url += $"&since={since:O}";
            }
            
            if (until.HasValue)
            {
                url += $"&until={until:O}";
            }
            
            Console.WriteLine($"[API] URL: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] GetCommitsAsync - Error: {response.StatusCode}");
                return new List<Commit>();
            }
            
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[API] GetCommitsAsync - Response length: {json.Length}");
            
            using (var jsonDoc = JsonDocument.Parse(json))
            {
                var commits = new List<Commit>();
                var elements = jsonDoc.RootElement.EnumerateArray();
                
                foreach (var element in elements)
                {
                    try
                    {
                        var commit = JsonSerializer.Deserialize<Commit>(element.GetRawText());
                        if (commit != null)
                        {
                            commits.Add(commit);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[API] GetCommitsAsync - Error deserializing commit: {ex.Message}");
                    }
                }
                
                Console.WriteLine($"[API] GetCommitsAsync - Successfully deserialized {commits.Count} commits");
                return commits;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API] GetCommitsAsync - ERROR: {ex.Message}");
            return new List<Commit>();
        }
    }
}

/// <summary>
/// Helper class to track upload progress
/// </summary>
public class ProgressStream : System.IO.Stream
{
    private readonly System.IO.Stream _innerStream;
    private readonly long _totalLength;
    private readonly IProgress<(long BytesRead, long TotalBytes)>? _progress;
    private long _bytesRead = 0;

    public ProgressStream(System.IO.Stream innerStream, long totalLength, IProgress<(long BytesRead, long TotalBytes)>? progress = null)
    {
        _innerStream = innerStream;
        _totalLength = totalLength;
        _progress = progress;
    }

    public override void Flush() => _innerStream.Flush();
    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = _innerStream.Read(buffer, offset, count);
        _bytesRead += bytesRead;
        _progress?.Report((_bytesRead, _totalLength));
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        _bytesRead += bytesRead;
        _progress?.Report((_bytesRead, _totalLength));
        return bytesRead;
    }

    public override long Seek(long offset, System.IO.SeekOrigin origin) => _innerStream.Seek(offset, origin);
    public override void SetLength(long value) => _innerStream.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;
    public override long Position { get => _innerStream.Position; set => _innerStream.Position = value; }
}
