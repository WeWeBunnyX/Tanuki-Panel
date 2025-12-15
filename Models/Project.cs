using System;
using System.Text.Json.Serialization;

namespace TanukiPanel.Models;

/// <summary>
/// Represents a GitLab project
/// </summary>
public class Project
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Visibility { get; set; } = "private"; // public, private, internal
    
    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }
    
    [JsonPropertyName("forks_count")]
    public int ForksCount { get; set; }
    
    [JsonPropertyName("open_issues_count")]
    public int OpenIssuesCount { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("last_activity_at")]
    public DateTime LastActivityAt { get; set; }
    
    public bool Archived { get; set; }
    
    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = "main";
    
    [JsonPropertyName("path_with_namespace")]
    public string PathWithNamespace { get; set; } = string.Empty;
}
