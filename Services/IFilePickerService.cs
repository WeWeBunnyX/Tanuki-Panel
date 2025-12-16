using System.Threading.Tasks;

namespace TanukiPanel.Services;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(string title = "Select a file");
}
