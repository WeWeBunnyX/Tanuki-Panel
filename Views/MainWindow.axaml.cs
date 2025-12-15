using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Media;
using TanukiPanel.Views;
using TanukiPanel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TanukiPanel.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Tanuki Panel - Your GitLab Ops Companion";
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.Parse("#F6F5F4")); // GNOME background

        // DataContext will be set by App.cs during dependency injection setup

        var contentControl = new ContentControl();
        contentControl.Bind(ContentProperty, new Binding("CurrentViewModel"));
        contentControl.ContentTemplate = new ViewLocator();

        // Get toast service from DI container
        var serviceProvider = (Application.Current as App)?.ServiceProvider;
        var toastService = serviceProvider?.GetService<IToastService>();

        // Create a grid to hold both content and toasts
        var mainGrid = new Grid();
        mainGrid.Children.Add(contentControl);
        
        if (toastService != null)
        {
            var toastContainer = new ToastContainer(toastService);
            mainGrid.Children.Add(toastContainer);
        }

        Content = mainGrid;
    }
}