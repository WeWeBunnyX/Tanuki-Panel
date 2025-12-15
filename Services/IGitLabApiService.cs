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
}
