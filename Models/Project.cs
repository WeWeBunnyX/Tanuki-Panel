using System;

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
    public string WebUrl { get; set; } = string.Empty;
    public int StarCount { get; set; }
    public int ForksCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public bool Archived { get; set; }
    public string DefaultBranch { get; set; } = "main";
    public string PathWithNamespace { get; set; } = string.Empty;
}
