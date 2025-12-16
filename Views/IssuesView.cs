using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Linq;
using TanukiPanel.Models;
using TanukiPanel.ViewModels;
using TanukiPanel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TanukiPanel.Views;

public class IssuesView : UserControl
{
    private IToastService? _toastService;

    public IssuesView()
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
        var gnomeRed = Color.Parse("#E01B24");

        // Main container using Grid (same as ProjectsView)
        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Tabs
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content fills

        // Header with back button
        var headerStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, VerticalAlignment = VerticalAlignment.Center };
        
        var backBtn = new Button
        {
            Content = "← Back",
            Padding = new Thickness(12, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        backBtn.Click += (s, e) =>
        {
            if (DataContext is ViewModelBase vm && vm is IssuesViewModel issuesVm && issuesVm.NavigationService != null)
            {
                issuesVm.NavigationService.GoBack();
            }
        };
        headerStack.Children.Add(backBtn);
        
        var headerBlock = new TextBlock
        {
            Text = "Issues",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText)
        };
        headerStack.Children.Add(headerBlock);

        var headerBorder = new Border
        {
            Child = headerStack,
            Margin = new Thickness(0, 0, 0, 16)
        };
        Grid.SetRow(headerBorder, 0);
        mainGrid.Children.Add(headerBorder);

        // Tabs
        var tabsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var projectIssuesButton = new Button
        {
            Content = "Project Issues",
            Padding = new Thickness(20, 10),
            FontSize = 13,
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6, 6, 0, 0)
        };

        var searchIssuesButton = new Button
        {
            Content = "Search Issues",
            Padding = new Thickness(20, 10),
            FontSize = 13,
            Background = new SolidColorBrush(gnomeSurface),
            Foreground = new SolidColorBrush(gnomeText),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            CornerRadius = new CornerRadius(6, 6, 0, 0)
        };

        var searchRepoButton = new Button
        {
            Content = "Search Repository",
            Padding = new Thickness(20, 10),
            FontSize = 13,
            Background = new SolidColorBrush(gnomeSurface),
            Foreground = new SolidColorBrush(gnomeText),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            CornerRadius = new CornerRadius(6, 6, 0, 0)
        };

        tabsPanel.Children.Add(projectIssuesButton);
        tabsPanel.Children.Add(searchIssuesButton);
        tabsPanel.Children.Add(searchRepoButton);

        Grid.SetRow(tabsPanel, 1);
        mainGrid.Children.Add(tabsPanel);

        // Create tab panels using helper methods
        var projectIssuesPanel = CreateProjectIssuesPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder, gnomeGreen, gnomeRed);
        var searchIssuesPanel = CreateSearchIssuesPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder, gnomeGreen, gnomeRed);
        var searchRepoPanel = CreateSearchRepositoryPanel(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder, gnomeGreen, gnomeRed);

        var tabContainer = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        tabContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        
        tabContainer.Children.Add(projectIssuesPanel);
        tabContainer.Children.Add(searchIssuesPanel);
        tabContainer.Children.Add(searchRepoPanel);
        Grid.SetRow(tabContainer, 2);
        mainGrid.Children.Add(tabContainer);

        // Tab switching
        projectIssuesButton.Click += (s, e) =>
        {
            projectIssuesButton.Background = new SolidColorBrush(gnomeBlue);
            projectIssuesButton.Foreground = Brushes.White;
            searchIssuesButton.Background = new SolidColorBrush(gnomeSurface);
            searchIssuesButton.Foreground = new SolidColorBrush(gnomeText);
            searchRepoButton.Background = new SolidColorBrush(gnomeSurface);
            searchRepoButton.Foreground = new SolidColorBrush(gnomeText);
            projectIssuesPanel.IsVisible = true;
            searchIssuesPanel.IsVisible = false;
            searchRepoPanel.IsVisible = false;
            if (DataContext is IssuesViewModel vm) vm.ViewMode = "ProjectIssues";
        };

        searchIssuesButton.Click += (s, e) =>
        {
            projectIssuesButton.Background = new SolidColorBrush(gnomeSurface);
            projectIssuesButton.Foreground = new SolidColorBrush(gnomeText);
            searchIssuesButton.Background = new SolidColorBrush(gnomeBlue);
            searchIssuesButton.Foreground = Brushes.White;
            searchRepoButton.Background = new SolidColorBrush(gnomeSurface);
            searchRepoButton.Foreground = new SolidColorBrush(gnomeText);
            projectIssuesPanel.IsVisible = false;
            searchIssuesPanel.IsVisible = true;
            searchRepoPanel.IsVisible = false;
            if (DataContext is IssuesViewModel vm) vm.ViewMode = "SearchIssues";
        };

        searchRepoButton.Click += (s, e) =>
        {
            projectIssuesButton.Background = new SolidColorBrush(gnomeSurface);
            projectIssuesButton.Foreground = new SolidColorBrush(gnomeText);
            searchIssuesButton.Background = new SolidColorBrush(gnomeSurface);
            searchIssuesButton.Foreground = new SolidColorBrush(gnomeText);
            searchRepoButton.Background = new SolidColorBrush(gnomeBlue);
            searchRepoButton.Foreground = Brushes.White;
            projectIssuesPanel.IsVisible = false;
            searchIssuesPanel.IsVisible = false;
            searchRepoPanel.IsVisible = true;
            if (DataContext is IssuesViewModel vm) vm.ViewMode = "SearchRepository";
        };

        Content = new Border
        {
            Child = mainGrid,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24)
        };
    }

    private Border CreateProjectIssuesPanel(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor, Color successColor, Color errorColor)
    {
        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var controls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(16, 16, 16, 0)
        };

        var searchBox = new TextBox
        {
            Watermark = "Search issues by title...",
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
        var stateCombo = new ComboBox
        {
            Width = 100,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1)
        };
        stateCombo.Items.Add("all");
        stateCombo.Items.Add("opened");
        stateCombo.Items.Add("closed");
        stateCombo.SelectedIndex = 0;
        stateCombo.Bind(ComboBox.SelectedItemProperty, new Binding("StateFilter") { Mode = BindingMode.TwoWay });
        filterRow.Children.Add(new TextBlock { Text = "State:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(subtextColor) });
        filterRow.Children.Add(stateCombo);

        var sortCombo = new ComboBox
        {
            Width = 120,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1)
        };
        sortCombo.Items.Add("UpdatedAt");
        sortCombo.Items.Add("CreatedAt");
        sortCombo.Items.Add("Title");
        sortCombo.SelectedIndex = 0;
        sortCombo.Bind(ComboBox.SelectedItemProperty, new Binding("SortBy") { Mode = BindingMode.TwoWay });
        filterRow.Children.Add(new TextBlock { Text = "Sort:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(subtextColor), Margin = new Thickness(12, 0, 0, 0) });
        filterRow.Children.Add(sortCombo);
        filterRow.Children.Add(new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch });

        var refreshBtn = new Button { Content = "Refresh", Padding = new Thickness(16, 8), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(accentColor), Foreground = Brushes.White };
        refreshBtn.Bind(Button.CommandProperty, new Binding("RefreshCommand"));
        filterRow.Children.Add(refreshBtn);

        controls.Children.Add(filterRow);
        Grid.SetRow(controls, 0);
        grid.Children.Add(controls);

        var listBox = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding("Issues"));
        listBox.ItemTemplate = CreateIssueTemplate(textColor, subtextColor, surfaceColor, accentColor, borderColor, successColor, errorColor);
        
        var scrollViewer = new ScrollViewer
        {
            Content = listBox,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(16, 12, 16, 16)
        };
        Grid.SetRow(scrollViewer, 1);
        grid.Children.Add(scrollViewer);

        return new Border
        {
            Child = grid,
            Background = new SolidColorBrush(surfaceColor),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(borderColor),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsVisible = true
        };
    }

    private Border CreateSearchIssuesPanel(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor, Color successColor, Color errorColor)
    {
        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var controls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(16, 16, 16, 0)
        };
        controls.Children.Add(new TextBlock { Text = "Search issues across all projects:", FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(textColor) });

        var searchRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var searchBox = new TextBox
        {
            Watermark = "Enter issue title or keywords...",
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
        searchBtn.Bind(Button.CommandProperty, new Binding("SearchIssuesCommand"));
        searchRow.Children.Add(searchBtn);

        controls.Children.Add(searchRow);
        Grid.SetRow(controls, 0);
        grid.Children.Add(controls);

        var listBox = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding("Issues"));
        listBox.ItemTemplate = CreateIssueTemplate(textColor, subtextColor, surfaceColor, accentColor, borderColor, successColor, errorColor);
        
        var scrollViewer = new ScrollViewer
        {
            Content = listBox,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(16, 12, 16, 16)
        };
        Grid.SetRow(scrollViewer, 1);
        grid.Children.Add(scrollViewer);

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

    private Border CreateSearchRepositoryPanel(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor, Color successColor, Color errorColor)
    {
        var grid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var controls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(16, 16, 16, 0)
        };
        controls.Children.Add(new TextBlock { Text = "Search issues in any repository:", FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(textColor) });

        var searchRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var repoPathBox = new TextBox
        {
            Watermark = "Enter repo path (e.g., group/project)...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(surfaceColor),
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(textColor)
        };
        repoPathBox.Bind(TextBox.TextProperty, new Binding("RepositoryPath") { Mode = BindingMode.TwoWay });
        searchRow.Children.Add(repoPathBox);

        var loadBtn = new Button { Content = "Load", Padding = new Thickness(16, 10), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(accentColor), Foreground = Brushes.White };
        loadBtn.Click += (s, e) =>
        {
            if (DataContext is IssuesViewModel vm)
                vm.LoadRepositoryIssuesCommand.Execute(null);
        };
        searchRow.Children.Add(loadBtn);

        controls.Children.Add(searchRow);
        Grid.SetRow(controls, 0);
        grid.Children.Add(controls);

        var listBox = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        listBox.Bind(ListBox.ItemsSourceProperty, new Binding("Issues"));
        listBox.ItemTemplate = CreateIssueTemplate(textColor, subtextColor, surfaceColor, accentColor, borderColor, successColor, errorColor);
        
        var scrollViewer = new ScrollViewer
        {
            Content = listBox,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(16, 12, 16, 16)
        };
        Grid.SetRow(scrollViewer, 1);
        grid.Children.Add(scrollViewer);

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

    private IDataTemplate CreateIssueTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor, Color successColor, Color errorColor)
    {
        return new FuncDataTemplate<Issue>((issue, _) =>
        {
            var contentStack = new StackPanel { Spacing = 8 };

            var titleStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            var titleBlock = new TextBlock
            {
                Text = issue?.Title ?? "Unknown",
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor),
                TextWrapping = TextWrapping.Wrap
            };
            titleStack.Children.Add(titleBlock);

            var stateTag = new Border
            {
                Child = new TextBlock
                {
                    Text = (issue?.State ?? "unknown").ToUpper(),
                    FontSize = 10,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brushes.White,
                    Margin = new Thickness(6, 2, 6, 2)
                },
                Background = new SolidColorBrush(issue?.State == "opened" ? successColor : errorColor),
                CornerRadius = new CornerRadius(4)
            };
            titleStack.Children.Add(stateTag);
            contentStack.Children.Add(titleStack);

            if (!string.IsNullOrEmpty(issue?.Description))
            {
                var descBlock = new TextBlock
                {
                    Text = issue.Description,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(subtextColor),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 600
                };
                contentStack.Children.Add(descBlock);
            }

            var infoStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            if (issue?.Author != null)
                infoStack.Children.Add(new TextBlock { Text = $"👤 {issue.Author.Name}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            infoStack.Children.Add(new TextBlock { Text = $"📅 {issue?.CreatedAt:MMM d, yyyy}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
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
            openBtn.Click += (s, e) => OpenIssue(issue);
            buttonsStack.Children.Add(openBtn);

            var toggleBtn = new Button
            {
                Content = issue?.State == "opened" ? "Close" : "Reopen",
                Padding = new Thickness(12, 6),
                FontSize = 10,
                Background = new SolidColorBrush(issue?.State == "opened" ? errorColor : successColor),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(4)
            };
            toggleBtn.Click += (s, e) =>
            {
                if (DataContext is IssuesViewModel vm)
                    vm.ToggleIssueStateCommand.Execute(issue);
            };
            buttonsStack.Children.Add(toggleBtn);

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

    private void OpenIssue(Issue? issue)
    {
        if (issue == null) return;
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo { UseShellExecute = true };
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                psi.FileName = issue.WebUrl;
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                psi.FileName = "open";
                psi.Arguments = issue.WebUrl;
            }
            else
            {
                psi.FileName = "xdg-open";
                psi.Arguments = issue.WebUrl;
            }
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open issue: {ex.Message}");
        }
    }
}
