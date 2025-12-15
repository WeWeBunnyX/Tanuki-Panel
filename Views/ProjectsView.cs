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

public class ProjectsView : UserControl
{
    private IToastService? _toastService;

    public ProjectsView()
    {
        var app = Application.Current as App;
        _toastService = app?.ServiceProvider?.GetService<IToastService>();

        var gnomeBlue = Color.Parse("#3584E4");
        var gnomeBackground = Color.Parse("#F6F5F4");
        var gnomeSurface = Color.Parse("#FFFFFF");
        var gnomeText = Color.Parse("#2E2E2E");
        var gnomeBorder = Color.Parse("#CCCCCC");
        var gnomeSubtext = Color.Parse("#77767B");

        // Main container using DockPanel
        var mainDockPanel = new DockPanel();

        // Header
        var headerBlock = new TextBlock
        {
            Text = "Projects",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 16)
        };
        DockPanel.SetDock(headerBlock, Dock.Top);
        mainDockPanel.Children.Add(headerBlock);

        // Tabs
        var tabsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var myProjectsButton = new Button
        {
            Content = "My Projects",
            Padding = new Thickness(20, 10),
            FontSize = 13,
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6, 6, 0, 0)
        };

        var searchProjectsButton = new Button
        {
            Content = "Search Projects",
            Padding = new Thickness(20, 10),
            FontSize = 13,
            Background = new SolidColorBrush(gnomeSurface),
            Foreground = new SolidColorBrush(gnomeText),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            CornerRadius = new CornerRadius(6, 6, 0, 0)
        };

        tabsPanel.Children.Add(myProjectsButton);
        tabsPanel.Children.Add(searchProjectsButton);

        DockPanel.SetDock(tabsPanel, Dock.Top);
        mainDockPanel.Children.Add(tabsPanel);

        // My Projects Tab - simple layout
        var myProjectsPanelInner = new DockPanel { LastChildFill = true };
        
        var myProjControls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        var searchBox = new TextBox
        {
            Watermark = "Search projects...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        searchBox.Bind(TextBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });
        myProjControls.Children.Add(searchBox);

        var controlsRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        
        var visibilityCombo = new ComboBox
        {
            Width = 110,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        visibilityCombo.Items.Add("All");
        visibilityCombo.Items.Add("public");
        visibilityCombo.Items.Add("private");
        visibilityCombo.Items.Add("internal");
        visibilityCombo.SelectedIndex = 0;
        visibilityCombo.Bind(ComboBox.SelectedItemProperty, new Binding("FilterVisibility") { Mode = BindingMode.TwoWay });
        controlsRow.Children.Add(new TextBlock { Text = "Visibility:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(gnomeSubtext) });
        controlsRow.Children.Add(visibilityCombo);

        var sortCombo = new ComboBox
        {
            Width = 130,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        sortCombo.Items.Add("LastActivity");
        sortCombo.Items.Add("Name");
        sortCombo.Items.Add("Stars");
        sortCombo.SelectedIndex = 0;
        sortCombo.Bind(ComboBox.SelectedItemProperty, new Binding("SortBy") { Mode = BindingMode.TwoWay });
        controlsRow.Children.Add(new TextBlock { Text = "Sort:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(gnomeSubtext), Margin = new Thickness(12, 0, 0, 0) });
        controlsRow.Children.Add(sortCombo);

        var hideArchived = new CheckBox { Content = "Hide Archived", FontSize = 12, IsChecked = true, Margin = new Thickness(12, 0, 0, 0), Foreground = new SolidColorBrush(gnomeText) };
        hideArchived.Bind(CheckBox.IsCheckedProperty, new Binding("HideArchived") { Mode = BindingMode.TwoWay });
        controlsRow.Children.Add(hideArchived);

        controlsRow.Children.Add(new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch });

        var refreshBtn = new Button { Content = "Refresh", Padding = new Thickness(16, 8), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(gnomeBlue), Foreground = Brushes.White };
        refreshBtn.Bind(Button.CommandProperty, new Binding("RefreshCommand"));
        controlsRow.Children.Add(refreshBtn);

        myProjControls.Children.Add(controlsRow);
        DockPanel.SetDock(myProjControls, Dock.Top);
        myProjectsPanelInner.Children.Add(myProjControls);

        // ListBox for projects - HAS NATIVE SCROLLING
        var myProjectsList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        myProjectsList.Bind(ListBox.ItemsSourceProperty, new Binding("Projects"));
        myProjectsList.ItemTemplate = CreateProjectTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder);
        myProjectsPanelInner.Children.Add(myProjectsList);

        var myProjectsPanel = new Border
        {
            Child = myProjectsPanelInner,
            Padding = new Thickness(16)
        };

        var myProjectsBorder = new Border
        {
            Child = myProjectsPanel,
            Background = new SolidColorBrush(gnomeSurface),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder)
        };

        // Search Projects Tab
        var searchPanelInner = new DockPanel { LastChildFill = true };
        
        var searchControls = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };
        searchControls.Children.Add(new TextBlock { Text = "Search other projects/repos:", FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(gnomeText) });

        var searchInputRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var searchQueryBox = new TextBox
        {
            Watermark = "Enter project name...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        searchQueryBox.Bind(TextBox.TextProperty, new Binding("SearchQuery") { Mode = BindingMode.TwoWay });
        searchInputRow.Children.Add(searchQueryBox);

        var searchBtn = new Button { Content = "Search", Padding = new Thickness(20, 10), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(gnomeBlue), Foreground = Brushes.White };
        searchBtn.Bind(Button.CommandProperty, new Binding("SearchProjectsCommand"));
        searchInputRow.Children.Add(searchBtn);

        searchControls.Children.Add(searchInputRow);
        DockPanel.SetDock(searchControls, Dock.Top);
        searchPanelInner.Children.Add(searchControls);

        // ListBox for search results - HAS NATIVE SCROLLING
        var searchResultsList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        searchResultsList.Bind(ListBox.ItemsSourceProperty, new Binding("Projects"));
        searchResultsList.ItemTemplate = CreateProjectTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder);
        searchPanelInner.Children.Add(searchResultsList);

        var searchPanel = new Border
        {
            Child = searchPanelInner,
            Padding = new Thickness(16)
        };

        var searchBorder = new Border
        {
            Child = searchPanel,
            Background = new SolidColorBrush(gnomeSurface),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            IsVisible = false
        };

        // Tab switching
        myProjectsButton.Click += (s, e) =>
        {
            myProjectsButton.Background = new SolidColorBrush(gnomeBlue);
            myProjectsButton.Foreground = Brushes.White;
            searchProjectsButton.Background = new SolidColorBrush(gnomeSurface);
            searchProjectsButton.Foreground = new SolidColorBrush(gnomeText);
            myProjectsBorder.IsVisible = true;
            searchBorder.IsVisible = false;
            if (DataContext is ProjectsViewModel vm) vm.ViewMode = "MyProjects";
        };

        searchProjectsButton.Click += (s, e) =>
        {
            myProjectsButton.Background = new SolidColorBrush(gnomeSurface);
            myProjectsButton.Foreground = new SolidColorBrush(gnomeText);
            searchProjectsButton.Background = new SolidColorBrush(gnomeBlue);
            searchProjectsButton.Foreground = Brushes.White;
            myProjectsBorder.IsVisible = false;
            searchBorder.IsVisible = true;
            if (DataContext is ProjectsViewModel vm) vm.ViewMode = "SearchProjects";
        };

        // Tab container
        var tabContainer = new Grid();
        tabContainer.Children.Add(myProjectsBorder);
        tabContainer.Children.Add(searchBorder);
        DockPanel.SetDock(tabContainer, Dock.Top);
        mainDockPanel.Children.Add(tabContainer);

        Content = new Border
        {
            Child = mainDockPanel,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24)
        };
    }

    private IDataTemplate CreateProjectTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor)
    {
        return new FuncDataTemplate<Models.Project>((project, _) =>
        {
            var contentStack = new StackPanel { Spacing = 8 };

            var nameBlock = new TextBlock
            {
                Text = project?.Name ?? "Unknown",
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor)
            };
            contentStack.Children.Add(nameBlock);

            if (!string.IsNullOrEmpty(project?.Description))
            {
                var descBlock = new TextBlock
                {
                    Text = project.Description,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(subtextColor),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 400
                };
                contentStack.Children.Add(descBlock);
            }

            var infoStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            infoStack.Children.Add(new TextBlock { Text = $"⭐ {project?.StarCount}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            infoStack.Children.Add(new TextBlock { Text = $"🔀 {project?.ForksCount}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            infoStack.Children.Add(new TextBlock { Text = project?.Visibility, FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            contentStack.Children.Add(infoStack);

            var buttonsStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 8, 0, 0) };

            var openBtn = new Button
            {
                Content = "Open",
                Padding = new Thickness(12, 6),
                FontSize = 10,
                Background = new SolidColorBrush(accentColor),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(4)
            };
            openBtn.Click += (s, e) => OpenProject(project);
            buttonsStack.Children.Add(openBtn);

            var copyBtn = new Button
            {
                Content = "Copy",
                Padding = new Thickness(12, 6),
                FontSize = 10,
                Background = new SolidColorBrush(surfaceColor),
                Foreground = new SolidColorBrush(textColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4)
            };
            copyBtn.Click += (s, e) => CopyUrl(project);
            buttonsStack.Children.Add(copyBtn);

            contentStack.Children.Add(buttonsStack);

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

    private void OpenProject(Project? project)
    {
        if (project == null) return;
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo { UseShellExecute = true };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                psi.FileName = project.WebUrl;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                psi.FileName = "open";
                psi.Arguments = project.WebUrl;
            }
            else
            {
                psi.FileName = "xdg-open";
                psi.Arguments = project.WebUrl;
            }
            System.Diagnostics.Process.Start(psi);
            _toastService?.ShowToast($"Opening {project.Name}...", ToastType.Info);
        }
        catch (Exception ex)
        {
            _toastService?.ShowToast($"Failed to open: {ex.Message}", ToastType.Error);
        }
    }

    private void CopyUrl(Project? project)
    {
        if (project == null) return;
        try
        {
            var url = $"git clone {project.WebUrl}.git";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c echo {url} | clip",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"echo '{url}' | pbcopy\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }
            else
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"echo -n '{url}' | xclip -selection clipboard\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }
            _toastService?.ShowToast($"Copied: {url}", ToastType.Success);
        }
        catch (Exception ex)
        {
            _toastService?.ShowToast($"Failed to copy: {ex.Message}", ToastType.Error);
        }
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
