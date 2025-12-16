using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using TanukiPanel.Models;
using TanukiPanel.Services;
using TanukiPanel.ViewModels;
using TanukiPanel.Views.Converters;

namespace TanukiPanel.Views;

public class PackageRegistryView : UserControl
{
    public PackageRegistryView()
    {
        // Wire up FilePickerService when the view is loaded
        this.AttachedToVisualTree += (s, e) =>
        {
            if (this.DataContext is PackageRegistryViewModel vm)
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var filePickerService = new FilePickerService(topLevel);
                    vm.Initialize(vm.GitLabService!, filePickerService);
                }
            }
        };

        var gnomeBlue = Color.Parse("#3584E4");
        var gnomeBackground = Color.Parse("#F6F5F4");
        var gnomeSurface = Color.Parse("#FFFFFF");
        var gnomeText = Color.Parse("#2E2E2E");
        var gnomeBorder = Color.Parse("#CCCCCC");
        var gnomeSubtext = Color.Parse("#77767B");
        var gnomeGreen = Color.Parse("#33D17A");
        var gnomeOrange = Color.Parse("#FF7F00");

        // Main DockPanel
        var mainDockPanel = new DockPanel
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            LastChildFill = true
        };

        // Header
        var headerBlock = new TextBlock
        {
            Text = "Package Registry",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 16)
        };
        DockPanel.SetDock(headerBlock, Dock.Top);
        mainDockPanel.Children.Add(headerBlock);

        // Repository input section
        var repoSection = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 16)
        };

        repoSection.Children.Add(new TextBlock
        {
            Text = "Select Repository",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeText)
        });

        var repoInputRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var repoInput = new TextBox
        {
            Watermark = "Enter repository path (e.g., group/project) or full URL...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        repoInput.Bind(TextBox.TextProperty, new Binding("RepositoryPath") { Mode = BindingMode.TwoWay });
        repoInputRow.Children.Add(repoInput);

        var selectBtn = new Button
        {
            Content = "Load Packages",
            Padding = new Thickness(20, 10),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        selectBtn.Bind(Button.CommandProperty, new Binding("LoadRepositoryCommand"));
        repoInputRow.Children.Add(selectBtn);

        var uploadBtn = new Button
        {
            Content = "⬆️ Upload Package",
            Padding = new Thickness(20, 10),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeOrange),
            Foreground = Brushes.White
        };
        uploadBtn.Bind(Button.CommandProperty, new Binding("UploadPackageCommand"));
        uploadBtn.Bind(IsEnabledProperty, new Binding("IsUploading") { Converter = new InvertBooleanConverter() });
        repoInputRow.Children.Add(uploadBtn);

        repoSection.Children.Add(repoInputRow);
        DockPanel.SetDock(repoSection, Dock.Top);
        mainDockPanel.Children.Add(repoSection);

        // Download progress section (shown when downloading)
        var downloadProgressSection = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E8F4FD")),
            BorderBrush = new SolidColorBrush(gnomeBlue),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 12),
            IsVisible = false
        };
        downloadProgressSection.Bind(IsVisibleProperty, new Binding("IsDownloading"));

        var downloadProgressStack = new StackPanel { Spacing = 8 };

        var downloadStatusBlock = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeText)
        };
        downloadStatusBlock.Bind(TextBlock.TextProperty, new Binding("DownloadFileName"));
        downloadProgressStack.Children.Add(downloadStatusBlock);

        var progressBar = new ProgressBar
        {
            Height = 20,
            CornerRadius = new CornerRadius(4)
        };
        progressBar.Bind(ProgressBar.ValueProperty, new Binding("DownloadProgress"));
        downloadProgressStack.Children.Add(progressBar);

        var progressTextBlock = new TextBlock
        {
            FontSize = 10,
            Foreground = new SolidColorBrush(gnomeSubtext)
        };
        progressTextBlock.Bind(TextBlock.TextProperty, new Binding("DownloadProgress") { Converter = new ProgressPercentageConverter() });
        downloadProgressStack.Children.Add(progressTextBlock);

        var cancelBtn = new Button
        {
            Content = "Cancel Download",
            Padding = new Thickness(12, 6),
            FontSize = 10,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.Parse("#E01B24")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 4, 0, 0)
        };
        cancelBtn.Bind(Button.CommandProperty, new Binding("CancelDownloadCommand"));
        downloadProgressStack.Children.Add(cancelBtn);

        downloadProgressSection.Child = downloadProgressStack;
        DockPanel.SetDock(downloadProgressSection, Dock.Top);
        mainDockPanel.Children.Add(downloadProgressSection);

        // Upload progress section (shown when uploading)
        var uploadProgressSection = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFF4E6")),
            BorderBrush = new SolidColorBrush(gnomeOrange),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 12),
            IsVisible = false
        };
        uploadProgressSection.Bind(IsVisibleProperty, new Binding("IsUploading"));

        var uploadProgressStack = new StackPanel { Spacing = 8 };

        var uploadStatusBlock = new TextBlock
        {
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeText)
        };
        uploadStatusBlock.Bind(TextBlock.TextProperty, new Binding("UploadFileName"));
        uploadProgressStack.Children.Add(uploadStatusBlock);

        var uploadProgressBar = new ProgressBar
        {
            Height = 20,
            CornerRadius = new CornerRadius(4)
        };
        uploadProgressBar.Bind(ProgressBar.ValueProperty, new Binding("UploadProgress"));
        uploadProgressStack.Children.Add(uploadProgressBar);

        var uploadProgressTextBlock = new TextBlock
        {
            FontSize = 10,
            Foreground = new SolidColorBrush(gnomeSubtext)
        };
        uploadProgressTextBlock.Bind(TextBlock.TextProperty, new Binding("UploadProgress") { Converter = new ProgressPercentageConverter() });
        uploadProgressStack.Children.Add(uploadProgressTextBlock);

        var uploadCancelBtn = new Button
        {
            Content = "Cancel Upload",
            Padding = new Thickness(12, 6),
            FontSize = 10,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Color.Parse("#E01B24")),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 4, 0, 0)
        };
        uploadCancelBtn.Bind(Button.CommandProperty, new Binding("CancelUploadCommand"));
        uploadProgressStack.Children.Add(uploadCancelBtn);

        uploadProgressSection.Child = uploadProgressStack;
        DockPanel.SetDock(uploadProgressSection, Dock.Top);
        mainDockPanel.Children.Add(uploadProgressSection);

        // Status bar
        var statusBar = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeSubtext),
            Margin = new Thickness(0, 12, 0, 0)
        };
        statusBar.Bind(TextBlock.TextProperty, new Binding("LoadingMessage"));
        DockPanel.SetDock(statusBar, Dock.Bottom);
        mainDockPanel.Children.Add(statusBar);

        // Packages list
        var packagesListSection = new Border
        {
            Background = new SolidColorBrush(gnomeSurface),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(8)
        };

        var listGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        listGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        listGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var headerBlock2 = new TextBlock
        {
            Text = "📦 Available Packages",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 12)
        };
        Grid.SetRow(headerBlock2, 0);
        listGrid.Children.Add(headerBlock2);

        var packagesList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        packagesList.Bind(ListBox.ItemsSourceProperty, new Binding("Packages"));
        packagesList.ItemTemplate = CreatePackageTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBorder, gnomeGreen);

        var scrollViewer = new ScrollViewer
        {
            Content = packagesList,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetRow(scrollViewer, 1);
        listGrid.Children.Add(scrollViewer);

        packagesListSection.Child = listGrid;
        mainDockPanel.Children.Add(packagesListSection);

        Content = new Border
        {
            Child = mainDockPanel,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24)
        };
    }

    private IDataTemplate CreatePackageTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color borderColor, Color successColor)
    {
        return new FuncDataTemplate<Package>((package, _) =>
        {
            var contentStack = new StackPanel { Spacing = 8 };

            // Package name and version
            var nameBlock = new TextBlock
            {
                Text = $"{package?.Name ?? "Unknown"} v{package?.Version ?? "0.0.0"}",
                FontSize = 13,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor),
                TextWrapping = TextWrapping.Wrap
            };
            contentStack.Children.Add(nameBlock);

            // Package type and created date
            var infoStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            infoStack.Children.Add(new TextBlock 
            { 
                Text = $"Type: {package?.PackageType ?? "unknown"}", 
                FontSize = 10, 
                Foreground = new SolidColorBrush(subtextColor) 
            });
            infoStack.Children.Add(new TextBlock 
            { 
                Text = $"📅 {GetTimeAgo(package?.CreatedAt ?? DateTime.Now)}", 
                FontSize = 10, 
                Foreground = new SolidColorBrush(subtextColor) 
            });
            contentStack.Children.Add(infoStack);

            // Download button
            var downloadBtn = new Button
            {
                Content = "⬇️ Download",
                Padding = new Thickness(12, 6),
                FontSize = 11,
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(successColor),
                Foreground = Brushes.White,
                Margin = new Thickness(0, 6, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            downloadBtn.Bind(IsEnabledProperty, new Binding("IsDownloading") { Converter = new InvertBooleanConverter() });
            
            downloadBtn.Click += (sender, e) =>
            {
                if (sender is Button btn && package != null)
                {
                    // Navigate up to find the PackageRegistryView (UserControl) which has the ViewModel as DataContext
                    var parent = btn.Parent;
                    while (parent != null && !(parent is PackageRegistryView))
                    {
                        parent = (parent as Control)?.Parent;
                    }
                    
                    if (parent is PackageRegistryView view && view.DataContext is PackageRegistryViewModel vm)
                    {
                        if (vm.DownloadPackageCommand.CanExecute(package))
                        {
                            vm.DownloadPackageCommand.Execute(package);
                        }
                    }
                }
            };
            contentStack.Children.Add(downloadBtn);

            return new Border
            {
                Child = contentStack,
                Background = new SolidColorBrush(Color.Parse("#F9F9F9")),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8)
            };
        });
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();
        if (timeSpan.TotalSeconds < 60) return "just now";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";
        return dateTime.ToString("MMM d, yyyy");
    }
}
