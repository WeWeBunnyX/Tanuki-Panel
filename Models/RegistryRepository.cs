using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TanukiPanel.Models;

/// <summary>
/// Represents a container registry repository
/// </summary>
public class RegistryRepository
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("project_id")]
    public int ProjectId { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("tags_count")]
    public int TagsCount { get; set; }

    [JsonPropertyName("delete_path")]
    public string DeletePath { get; set; } = string.Empty;
}

/// <summary>
/// Represents a container registry image tag
/// </summary>
public class RegistryTag
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("digest")]
    public string Digest { get; set; } = string.Empty;

    [JsonPropertyName("revision")]
    public string Revision { get; set; } = string.Empty;

    [JsonPropertyName("short_revision")]
    public string ShortRevision { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("total_size")]
    public long TotalSize { get; set; }
}

