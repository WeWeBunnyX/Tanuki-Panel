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
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[API] ERROR: Project not found (Status: {response.StatusCode})");
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
    }}