using System;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Input;

namespace TanukiPanel.ViewModels;

public class ApiKeyViewModel : ViewModelBase
{
    private string? _apiKey;
    public string? ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public IRelayCommand SaveCommand { get; }

    public ApiKeyViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
    }

    private void OnSave()
    {
        var apiKey = ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("No API key entered. Nothing saved.");
            return;
        }

        var model = new
        {
            ApiKey = apiKey,
            SavedAtUtc = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(model, options);

        try
        {
            var repoRoot = FindRepoRoot();
            string path;

            if (!string.IsNullOrEmpty(repoRoot))
            {
                path = Path.Combine(repoRoot, "tanuki_api_key.json");
            }
            else
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "TanukiPanel");
                Directory.CreateDirectory(dir);
                path = Path.Combine(dir, "tanuki_api_key.json");
            }

            File.WriteAllText(path, json);
            Console.WriteLine($"Saved API key to {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save API key: {ex.Message}");
        }
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
