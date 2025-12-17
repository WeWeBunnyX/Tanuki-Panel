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

        // Main container
        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Tabs
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content fills

        // Header
        var headerBlock = new TextBlock
        {
            Text = "Projects",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 16)
        };
        Grid.SetRow(headerBlock, 0);
        mainGrid.Children.Add(headerBlock);

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
        Grid.SetRow(tabsPanel, 1);
        mainGrid.Children.Add(tabsPanel);

        // SIMPLE MY PROJECTS TAB
        var myProjectsPanel = CreateMyProjectsPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder);
        
        // SIMPLE SEARCH PROJECTS TAB
        var searchProjectsPanel = CreateSearchProjectsPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder);

        var tabContainer = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        tabContainer.Children.Add(myProjectsPanel);
        tabContainer.Children.Add(searchProjectsPanel);
        
        Grid.SetRow(tabContainer, 2);
        mainGrid.Children.Add(tabContainer);

        // Tab switching
        myProjectsButton.Click += (s, e) =>
        {
            myProjectsButton.Background = new SolidColorBrush(gnomeBlue);
            myProjectsButton.Foreground = Brushes.White;
            searchProjectsButton.Background = new SolidColorBrush(gnomeSurface);
            searchProjectsButton.Foreground = new SolidColorBrush(gnomeText);
            myProjectsPanel.IsVisible = true;
            searchProjectsPanel.IsVisible = false;
            if (DataContext is ProjectsViewModel vm) vm.ViewMode = "MyProjects";
        };

        searchProjectsButton.Click += (s, e) =>
        {
            myProjectsButton.Background = new SolidColorBrush(gnomeSurface);
            myProjectsButton.Foreground = new SolidColorBrush(gnomeText);
            searchProjectsButton.Background = new SolidColorBrush(gnomeBlue);
            searchProjectsButton.Foreground = Brushes.White;
            myProjectsPanel.IsVisible = false;
            searchProjectsPanel.IsVisible = true;
            if (DataContext is ProjectsViewModel vm) vm.ViewMode = "SearchProjects";
        };

        Content = new Border
        {
            Child = mainGrid,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }

    private Border CreateMyProjectsPanel(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor)
    {
        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Controls
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // List (takes remaining space)
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Pagination

        // Controls section (Row 0)
        var controls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(16, 16, 16, 12)
        };

        var searchBox = new TextBox
        {
            Watermark = "Search projects...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(textColor)
        };
        searchBox.Bind(TextBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });
        controls.Children.Add(searchBox);

        var filterRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        
        var visibilityCombo = new ComboBox
        {
            Width = 110,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(textColor)
        };
        visibilityCombo.Items.Add("All");
        visibilityCombo.Items.Add("public");
        visibilityCombo.Items.Add("private");
        visibilityCombo.Items.Add("internal");
        visibilityCombo.SelectedIndex = 0;
        visibilityCombo.Bind(ComboBox.SelectedItemProperty, new Binding("FilterVisibility") { Mode = BindingMode.TwoWay });
        filterRow.Children.Add(new TextBlock { Text = "Visibility:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(subtextColor) });
        filterRow.Children.Add(visibilityCombo);

        var sortCombo = new ComboBox
        {
            Width = 130,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(textColor)
        };
        sortCombo.Items.Add("LastActivity");
        sortCombo.Items.Add("Name");
        sortCombo.Items.Add("Stars");
        sortCombo.SelectedIndex = 0;
        sortCombo.Bind(ComboBox.SelectedItemProperty, new Binding("SortBy") { Mode = BindingMode.TwoWay });
        filterRow.Children.Add(new TextBlock { Text = "Sort:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(subtextColor), Margin = new Thickness(12, 0, 0, 0) });
        filterRow.Children.Add(sortCombo);

        var hideArchived = new CheckBox { Content = "Hide Archived", FontSize = 12, IsChecked = true, Margin = new Thickness(12, 0, 0, 0), Foreground = new SolidColorBrush(textColor) };
        hideArchived.Bind(CheckBox.IsCheckedProperty, new Binding("HideArchived") { Mode = BindingMode.TwoWay });
        filterRow.Children.Add(hideArchived);

        filterRow.Children.Add(new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch });

        var refreshBtn = new Button { Content = "Refresh", Padding = new Thickness(16, 8), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(accentColor), Foreground = Brushes.White };
        refreshBtn.Bind(Button.CommandProperty, new Binding("RefreshCommand"));
        filterRow.Children.Add(refreshBtn);

        controls.Children.Add(filterRow);
        Grid.SetRow(controls, 0);
        mainGrid.Children.Add(controls);

        // Projects list (Row 1 - fills remaining space) - NO ScrollViewer, just ListBox
        var listBox = new ListBox
        {
            Padding = new Thickness(16, 12, 16, 12),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding("Projects"));
        listBox.ItemTemplate = CreateProjectTemplate(textColor, subtextColor, surfaceColor, accentColor, borderColor);
        Grid.SetRow(listBox, 1);
        mainGrid.Children.Add(listBox);

        // Pagination controls
        var paginationStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(16, 12, 16, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var prevBtn = new Button
        {
            Content = "← Previous",
            Padding = new Thickness(12, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(accentColor),
            Foreground = Brushes.White
        };
        prevBtn.Bind(Button.CommandProperty, new Binding("PreviousMyPageCommand"));
        prevBtn.Bind(Button.IsEnabledProperty, new Binding("HasPreviousMyPage"));
        paginationStack.Children.Add(prevBtn);

        var pageInfoBlock = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(subtextColor),
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200
        };
        pageInfoBlock.Bind(TextBlock.TextProperty, new Binding("MyProjectsPageInfo"));
        paginationStack.Children.Add(pageInfoBlock);

        var nextBtn = new Button
        {
            Content = "Next →",
            Padding = new Thickness(12, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(accentColor),
            Foreground = Brushes.White
        };
        nextBtn.Bind(Button.CommandProperty, new Binding("NextMyPageCommand"));
        nextBtn.Bind(Button.IsEnabledProperty, new Binding("HasNextMyPage"));
        paginationStack.Children.Add(nextBtn);

        Grid.SetRow(paginationStack, 2);
        mainGrid.Children.Add(paginationStack);

        return new Border
        {
            Child = mainGrid,
            Background = new SolidColorBrush(surfaceColor),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(borderColor),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }

    private Border CreateSearchProjectsPanel(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor)
    {
        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Controls
        var controls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(16, 16, 16, 0)
        };
        controls.Children.Add(new TextBlock { Text = "Search other projects/repos:", FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(textColor) });

        var searchRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var searchBox = new TextBox
        {
            Watermark = "Enter project name...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(textColor)
        };
        searchBox.Bind(TextBox.TextProperty, new Binding("SearchQuery") { Mode = BindingMode.TwoWay });
        searchRow.Children.Add(searchBox);

        var searchBtn = new Button { Content = "Search", Padding = new Thickness(20, 10), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(accentColor), Foreground = Brushes.White };
        searchBtn.Bind(Button.CommandProperty, new Binding("SearchProjectsCommand"));
        searchRow.Children.Add(searchBtn);

        controls.Children.Add(searchRow);
        Grid.SetRow(controls, 0);
        grid.Children.Add(controls);

        // Scrollable list (Row 1 - fills space)
        var listBox = new ListBox
        {
            Padding = new Thickness(16, 12, 16, 12),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MaxHeight = 400
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding("Projects"));
        listBox.ItemTemplate = CreateProjectTemplate(textColor, subtextColor, surfaceColor, accentColor, borderColor);
        Grid.SetRow(listBox, 1);
        grid.Children.Add(listBox);

        // Pagination (Row 2)
        var pagination = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(16, 12, 16, 12),
            VerticalAlignment = VerticalAlignment.Center
        };

        var prevBtn = new Button { Content = "← Previous", Padding = new Thickness(16, 8), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(accentColor), Foreground = Brushes.White };
        prevBtn.Bind(Button.CommandProperty, new Binding("PreviousPageCommand"));
        pagination.Children.Add(prevBtn);

        var pageInfo = new TextBlock { FontSize = 12, Foreground = new SolidColorBrush(textColor), VerticalAlignment = VerticalAlignment.Center };
        pageInfo.Bind(TextBlock.TextProperty, new Binding("PaginationInfo"));
        pagination.Children.Add(pageInfo);

        var nextBtn = new Button { Content = "Next →", Padding = new Thickness(16, 8), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(accentColor), Foreground = Brushes.White };
        nextBtn.Bind(Button.CommandProperty, new Binding("NextPageCommand"));
        pagination.Children.Add(nextBtn);

        Grid.SetRow(pagination, 2);
        grid.Children.Add(pagination);

        return new Border
        {
            Child = grid,
            Background = new SolidColorBrush(surfaceColor),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(borderColor),
            IsVisible = false,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
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

            var issuesBtn = new Button
            {
                Content = "Issues",
                Padding = new Thickness(12, 6),
                FontSize = 10,
                Background = new SolidColorBrush(accentColor),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(4)
            };
            issuesBtn.Click += (s, e) => ViewIssues(project);
            buttonsStack.Children.Add(issuesBtn);

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

    private void ViewIssues(Project? project)
    {
        if (project == null || DataContext is not ProjectsViewModel vm) return;
        
        try
        {
            vm.ViewIssuesCommand.Execute(project);
            _toastService?.ShowToast($"Loading issues for {project.Name}...", ToastType.Info);
        }
        catch (Exception ex)
        {
            _toastService?.ShowToast($"Failed to load issues: {ex.Message}", ToastType.Error);
        }
    }
}
