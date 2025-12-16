using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
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
}
