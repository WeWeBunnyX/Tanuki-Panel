using System;
using System.Text.Json.Serialization;

namespace TanukiPanel.Models;

/// <summary>
/// Represents a GitLab issue
/// </summary>
public class Issue
{
    public int Id { get; set; }
    
    public int IssueIid { get; set; }
    
    [JsonPropertyName("iid")]
    public int Iid { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string State { get; set; } = "opened"; // opened, closed, locked, all
    
    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public Author? Author { get; set; }
    
    [JsonPropertyName("assignee")]
    public Author? Assignee { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }
    
    public int? Weight { get; set; }
    
    [JsonPropertyName("upvotes")]
    public int Upvotes { get; set; }
    
    [JsonPropertyName("downvotes")]
    public int Downvotes { get; set; }
    
    public string[] Labels { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Represents an author/assignee in GitLab
/// </summary>
public class Author
{
    public int Id { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; } = string.Empty;
}

