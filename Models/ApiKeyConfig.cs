using System;

namespace TanukiPanel.Models;

/// <summary>
/// Domain model for storing API key configuration.
/// </summary>
public class ApiKeyConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public DateTime SavedAtUtc { get; set; }
}
