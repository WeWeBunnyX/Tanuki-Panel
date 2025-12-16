using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace TanukiPanel.Services;

public class FilePickerService : IFilePickerService
{
    private readonly TopLevel _topLevel;

    public FilePickerService(TopLevel topLevel)
    {
        _topLevel = topLevel ?? throw new ArgumentNullException(nameof(topLevel));
    }

    public async Task<string?> PickFileAsync(string title = "Select a file")
    {
        try
        {
            Console.WriteLine($"[FilePickerService] Opening file picker with title: {title}");
            
            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });

            if (files != null && files.Count > 0)
            {
                string selectedPath = files[0].Path.LocalPath;
                Console.WriteLine($"[FilePickerService] File selected: {selectedPath}");
                return selectedPath;
            }

            Console.WriteLine($"[FilePickerService] No file selected");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FilePickerService] ERROR: {ex.Message}");
            Console.WriteLine($"[FilePickerService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}
