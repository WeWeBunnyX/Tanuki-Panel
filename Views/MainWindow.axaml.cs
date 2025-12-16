using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using TanukiPanel.Views;
using TanukiPanel.Services;
using TanukiPanel.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TanukiPanel.Views;

public partial class MainWindow : Window
{
    private ContentControl? _contentControl;
    private ImageBrush? _avatarBrush;
    private TextBlock? _userNameBlock;
    
    public MainWindow()
    {
        Title = "Tanuki Panel - Your GitLab Ops Companion";
        Width = 800;
        Height = 600;
        Background = new SolidColorBrush(Color.Parse("#F6F5F4")); // GNOME background

        _contentControl = new ContentControl();
        _contentControl.ContentTemplate = new ViewLocator();

        // Get toast service from DI container
        var serviceProvider = (Application.Current as App)?.ServiceProvider;
        var toastService = serviceProvider?.GetService<IToastService>();

        // Create a grid to hold content and overlay header
        var mainGrid = new Grid();

        // Create content area
        var contentGrid = new Grid();
        contentGrid.Children.Add(_contentControl);
        
        if (toastService != null)
        {
            var toastContainer = new ToastContainer(toastService);
            contentGrid.Children.Add(toastContainer);
        }
        mainGrid.Children.Add(contentGrid);

        // Create user info header overlay in top right
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Spacing = 8,
            Margin = new Thickness(0, 12, 24, 0)
        };

        // User name TextBlock
        _userNameBlock = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#2E2E2E")),
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(_userNameBlock);

        // Add rounded border for avatar with background image
        var avatarBorder = new Border
        {
            Width = 48,
            Height = 48,
            CornerRadius = new CornerRadius(24),
            ClipToBounds = true,
            BorderBrush = new SolidColorBrush(Color.Parse("#CCCCCC")),
            BorderThickness = new Thickness(1)
        };
        
        // Use ImageBrush for remote URL avatar
        _avatarBrush = new ImageBrush();
        avatarBorder.Background = _avatarBrush;
        headerPanel.Children.Add(avatarBorder);

        mainGrid.Children.Add(headerPanel);
        Content = mainGrid;
        
        // Set up bindings after window is loaded and DataContext is available
        this.Loaded += async (s, e) =>
        {
            try
            {
                if (_contentControl != null && _userNameBlock != null && _avatarBrush != null)
                {
                    // Use RelativeSource binding instead to ensure we get the DataContext
                    _contentControl.Bind(ContentProperty, new Binding("CurrentViewModel") { Source = this.DataContext });
                    _userNameBlock.Bind(TextBlock.TextProperty, new Binding("CurrentUserName") { Source = this.DataContext });
                    
                    // For avatar, download and save locally
                    var dataContext = this.DataContext;
                    if (dataContext is MainWindowViewModel vm && !string.IsNullOrEmpty(vm.CurrentUserAvatarUrl))
                    {
                        await LoadAndSetAvatarAsync(vm.CurrentUserAvatarUrl);
                    }
                    
                    // Also subscribe to changes
                    if (dataContext is MainWindowViewModel viewModel)
                    {
                        viewModel.PropertyChanged += async (vm, args) =>
                        {
                            if (args.PropertyName == "CurrentUserAvatarUrl" && !string.IsNullOrEmpty(viewModel.CurrentUserAvatarUrl))
                            {
                                await LoadAndSetAvatarAsync(viewModel.CurrentUserAvatarUrl);
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error binding: {ex}");
            }
        };
    }
    
    private async Task LoadAndSetAvatarAsync(string avatarUrl)
    {
        try
        {
            // Download the avatar image
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                var response = await httpClient.GetAsync(avatarUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    
                    // Save to local directory
                    var localPath = System.IO.Path.Combine(
                        System.IO.Directory.GetCurrentDirectory(),
                        "avatar.png"
                    );
                    
                    await System.IO.File.WriteAllBytesAsync(localPath, imageBytes);
                    
                    System.Diagnostics.Debug.WriteLine($"Avatar saved to: {localPath}");
                    
                    // Load from local path
                    var bitmap = new Bitmap(localPath);
                    _avatarBrush!.Source = bitmap;
                    
                    System.Diagnostics.Debug.WriteLine($"Avatar loaded successfully from: {localPath}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load avatar: {ex}");
        }
    }
}