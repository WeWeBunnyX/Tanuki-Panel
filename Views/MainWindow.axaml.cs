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

        var contentHost = new Grid();

        var welcome = new WelcomeView();
        contentHost.Children.Add(welcome);

        welcome.Finished += async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                contentHost.Children.Clear();
                var apiView = new ApiKeyView();
                contentHost.Children.Add(apiView);
            });
        };

        Content = contentHost;
    }
}