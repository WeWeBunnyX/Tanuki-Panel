using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Threading;
using TanukiPanel.Views;

namespace TanukiPanel.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
    Title = "Tanuki Panel - Your GitLab Ops Companion";
        Width = 800;
        Height = 600;

        // DataContext will be set by App.cs during dependency injection setup

        var contentControl = new ContentControl();
        contentControl.Bind(ContentProperty, new Binding("CurrentViewModel"));
        contentControl.ContentTemplate = new ViewLocator();

        Content = contentControl;
    }
}