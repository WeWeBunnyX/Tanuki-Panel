using System;
using System.Text.Json.Serialization;

namespace TanukiPanel.Models;

/// <summary>
/// Represents a GitLab commit
/// </summary>
public class Commit
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("short_id")]
    public string ShortId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("author_email")]
    public string AuthorEmail { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("parent_ids")]
    public string[] ParentIds { get; set; } = Array.Empty<string>();

    [JsonPropertyName("web_url")]
    public string WebUrl { get; set; } = string.Empty;
}
