using System.Collections.ObjectModel;

namespace TanukiPanel.ViewModels;

public class SideBarContentViewModel : ViewModelBase
{
    private string _title = "Main Dashboard";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
