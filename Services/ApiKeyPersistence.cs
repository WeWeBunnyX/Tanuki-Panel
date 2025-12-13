using System;
using System.IO;
using System.Text.Json;
using TanukiPanel.Models;

namespace TanukiPanel.Services;

/// <summary>
/// Implements API key persistence to JSON file.
/// </summary>
public class ApiKeyPersistence : IApiKeyPersistence
{
    public bool SaveApiKey(string apiKey)
    {
        var trimmedKey = apiKey?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedKey))
        {
            Console.WriteLine("No API key entered. Nothing saved.");
            return false;
        }

        var config = new ApiKeyConfig
        {
            ApiKey = trimmedKey,
            SavedAtUtc = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(config, options);

        try
        {
            var path = GetApiKeyPath();
            var directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, json);
            Console.WriteLine($"Saved API key to {path}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save API key: {ex.Message}");
            return false;
        }
    }

    public ApiKeyConfig? LoadApiKey()
    {
        try
        {
            var path = GetApiKeyPath();
            if (!File.Exists(path))
            {
                return null;
            }

            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<ApiKeyConfig>(json);
            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load API key: {ex.Message}");
            return null;
        }
    }

    private static string GetApiKeyPath()
    {
        var repoRoot = FindRepoRoot();

        if (!string.IsNullOrEmpty(repoRoot))
        {
            return Path.Combine(repoRoot, "tanuki_api_key.json");
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "TanukiPanel");
        return Path.Combine(dir, "tanuki_api_key.json");
    }

    private static string? FindRepoRoot()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var start in starts)
        {
            if (string.IsNullOrEmpty(start))
                continue;

            var dir = new DirectoryInfo(start);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;
                dir = dir.Parent;
            }
        }

        return null;
    }
}
