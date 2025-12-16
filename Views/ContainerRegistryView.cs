using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using TanukiPanel.Models;
using TanukiPanel.ViewModels;
using TanukiPanel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TanukiPanel.Views;

public class ContainerRegistryView : UserControl
{
    private IToastService? _toastService;

    public ContainerRegistryView()
    {
        var app = Application.Current as App;
        _toastService = app?.ServiceProvider?.GetService<IToastService>();

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
            Text = "Container Registry",
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
            Watermark = "Enter repository path (e.g., group/project)...",
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
            Content = "Load Registry",
            Padding = new Thickness(20, 10),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        selectBtn.Bind(Button.CommandProperty, new Binding("LoadRepositoryCommand"));
        repoInputRow.Children.Add(selectBtn);

        repoSection.Children.Add(repoInputRow);
        DockPanel.SetDock(repoSection, Dock.Top);
        mainDockPanel.Children.Add(repoSection);

        // Content area
        var contentGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });

        // Left panel: Registries and Tags
        var leftPanel = CreateLeftPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder, gnomeGreen);
        Grid.SetColumn(leftPanel, 0);
        contentGrid.Children.Add(leftPanel);

        // Right panel: Logs (hidden by default)
        var rightPanel = CreateLogsPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBorder);
        rightPanel.Bind(IsVisibleProperty, new Binding("ShowLogs"));
        Grid.SetColumn(rightPanel, 1);
        contentGrid.Children.Add(rightPanel);

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

        mainDockPanel.Children.Add(contentGrid);

        Content = new Border
        {
            Child = mainDockPanel,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24)
        };
    }

    private Border CreateLeftPanel(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor, Color successColor)
    {
        var mainPanel = new DockPanel
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            LastChildFill = true
        };

        // Registries Section Header
        var registriesHeader = new TextBlock
        {
            Text = "🏗️ Registries",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(textColor),
            Margin = new Thickness(0, 0, 0, 12)
        };
        DockPanel.SetDock(registriesHeader, Dock.Top);
        mainPanel.Children.Add(registriesHeader);

        // Registries List
        var registriesList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        registriesList.Bind(ListBox.ItemsSourceProperty, new Binding("Registries"));
        registriesList.Bind(ListBox.SelectedItemProperty, new Binding("SelectedRegistry") { Mode = BindingMode.TwoWay });
        registriesList.ItemTemplate = CreateRegistryTemplate(textColor, subtextColor, surfaceColor, borderColor);

        var registriesScroller = new ScrollViewer
        {
            Content = registriesList,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        mainPanel.Children.Add(registriesScroller);

        return new Border
        {
            Child = mainPanel,
            Background = new SolidColorBrush(surfaceColor),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(borderColor)
        };
    }

    private Border CreateLogsPanel(Color textColor, Color subtextColor, Color surfaceColor, Color borderColor)
    {
        var mainPanel = new DockPanel
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            LastChildFill = true
        };

        // Header
        var header = new TextBlock
        {
            Text = "📋 Tag Logs",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(textColor),
            Margin = new Thickness(0, 0, 0, 12)
        };
        DockPanel.SetDock(header, Dock.Top);
        mainPanel.Children.Add(header);

        // Close button
        var closeBtn = new Button
        {
            Content = "✕",
            Padding = new Thickness(8, 4),
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Right,
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(subtextColor)
        };
        closeBtn.Click += (s, e) =>
        {
            if (DataContext is ContainerRegistryViewModel vm)
            {
                vm.ShowLogs = false;
            }
        };
        DockPanel.SetDock(closeBtn, Dock.Top);
        mainPanel.Children.Add(closeBtn);

        // Logs TextBox
        var logsBox = new TextBox
        {
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(12),
            FontFamily = new FontFamily("Courier New"),
            FontSize = 10,
            Background = new SolidColorBrush(surfaceColor),
            Foreground = new SolidColorBrush(textColor),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        logsBox.Bind(TextBox.TextProperty, new Binding("TagLogs"));

        var logsScroller = new ScrollViewer
        {
            Content = logsBox,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
        };
        mainPanel.Children.Add(logsScroller);

        return new Border
        {
            Child = mainPanel,
            Background = new SolidColorBrush(surfaceColor),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(borderColor),
            Margin = new Thickness(12, 0, 0, 0)
        };
    }

    private IDataTemplate CreateRegistryTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color borderColor)
    {
        return new FuncDataTemplate<RegistryRepository>((registry, _) =>
        {
            var contentStack = new StackPanel { Spacing = 8 };

            var nameBlock = new TextBlock
            {
                Text = registry?.Name ?? "Unknown",
                FontSize = 13,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor),
                TextWrapping = TextWrapping.Wrap
            };
            contentStack.Children.Add(nameBlock);

            var pathBlock = new TextBlock
            {
                Text = registry?.Path ?? "",
                FontSize = 10,
                Foreground = new SolidColorBrush(subtextColor),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0)
            };
            contentStack.Children.Add(pathBlock);

            var infoStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            infoStack.Children.Add(new TextBlock { Text = $"📦 {registry?.TagsCount} tags", FontSize = 9, Foreground = new SolidColorBrush(subtextColor) });
            infoStack.Children.Add(new TextBlock { Text = $"📅 {GetTimeAgo(registry?.CreatedAt ?? DateTime.Now)}", FontSize = 9, Foreground = new SolidColorBrush(subtextColor) });
            contentStack.Children.Add(infoStack);

            // Tags list for this registry
            if (DataContext is ContainerRegistryViewModel vm && vm.SelectedRegistry?.Id == registry?.Id)
            {
                contentStack.Children.Add(new TextBlock
                {
                    Text = "Tags:",
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = new SolidColorBrush(textColor),
                    Margin = new Thickness(0, 8, 0, 4)
                });

                var tagsList = new ListBox
                {
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0),
                    MaxHeight = 200
                };
                tagsList.Bind(ListBox.ItemsSourceProperty, new Binding("Tags"));
                tagsList.ItemTemplate = CreateTagTemplate(textColor, subtextColor, surfaceColor, borderColor);

                var tagsScroller = new ScrollViewer
                {
                    Content = tagsList,
                    VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
                };
                contentStack.Children.Add(tagsScroller);
            }

            return new Border
            {
                Child = contentStack,
                Background = new SolidColorBrush(surfaceColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8)
            };
        });
    }

    private IDataTemplate CreateTagTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color borderColor)
    {
        return new FuncDataTemplate<RegistryTag>((tag, _) =>
        {
            var contentStack = new StackPanel { Spacing = 6 };

            var nameBlock = new TextBlock
            {
                Text = tag?.Name ?? "Unknown",
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor),
                TextWrapping = TextWrapping.Wrap
            };
            contentStack.Children.Add(nameBlock);

            var infoStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
            infoStack.Children.Add(new TextBlock { Text = $"Size: {FormatBytes(tag?.TotalSize ?? 0)}", FontSize = 9, Foreground = new SolidColorBrush(subtextColor) });
            infoStack.Children.Add(new TextBlock { Text = $"Created: {GetTimeAgo(tag?.CreatedAt ?? DateTime.Now)}", FontSize = 9, Foreground = new SolidColorBrush(subtextColor) });
            contentStack.Children.Add(infoStack);

            var buttonsStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 6, 0, 0) };

            var logsBtn = new Button
            {
                Content = "📋 Logs",
                Padding = new Thickness(10, 4),
                FontSize = 9,
                Background = new SolidColorBrush(Color.Parse("#3584E4")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(3)
            };
            logsBtn.Bind(Button.CommandProperty, new Binding("ViewTagLogsCommand"));
            logsBtn.CommandParameter = tag;
            buttonsStack.Children.Add(logsBtn);

            var deleteBtn = new Button
            {
                Content = "🗑️ Delete",
                Padding = new Thickness(10, 4),
                FontSize = 9,
                Background = new SolidColorBrush(Color.Parse("#E01B24")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(3)
            };
            deleteBtn.Bind(Button.CommandProperty, new Binding("DeleteTagCommand"));
            deleteBtn.CommandParameter = tag;
            buttonsStack.Children.Add(deleteBtn);

            contentStack.Children.Add(buttonsStack);

            return new Border
            {
                Child = contentStack,
                Background = new SolidColorBrush(Color.Parse("#F9F9F9")),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 4)
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

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

