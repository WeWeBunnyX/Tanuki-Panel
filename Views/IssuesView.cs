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

        // Main container using DockPanel
        var mainDockPanel = new DockPanel();

        // Header with project name
        var headerStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, VerticalAlignment = VerticalAlignment.Center };
        
        var headerBlock = new TextBlock
        {
            Text = "Issues",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText)
        };
        headerStack.Children.Add(headerBlock);

        var projectNameBlock = new TextBlock
        {
            Text = "",
            FontSize = 14,
            Foreground = new SolidColorBrush(gnomeSubtext),
            VerticalAlignment = VerticalAlignment.Center
        };
        projectNameBlock.Bind(TextBlock.TextProperty, new Binding("CurrentProject.Name"));
        headerStack.Children.Add(projectNameBlock);

        var headerBorder = new Border
        {
            Child = headerStack,
            Margin = new Thickness(0, 0, 0, 16)
        };
        DockPanel.SetDock(headerBorder, Dock.Top);
        mainDockPanel.Children.Add(headerBorder);

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

        tabsPanel.Children.Add(projectIssuesButton);
        tabsPanel.Children.Add(searchIssuesButton);

        DockPanel.SetDock(tabsPanel, Dock.Top);
        mainDockPanel.Children.Add(tabsPanel);

        // Project Issues Tab
        var projectIssuesPanelInner = new DockPanel { LastChildFill = true };
        
        var projIssuesControls = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        var searchBox = new TextBox
        {
            Watermark = "Search issues by title...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        searchBox.Bind(TextBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });
        projIssuesControls.Children.Add(searchBox);

        var controlsRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        
        var stateCombo = new ComboBox
        {
            Width = 100,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        stateCombo.Items.Add("all");
        stateCombo.Items.Add("opened");
        stateCombo.Items.Add("closed");
        stateCombo.SelectedIndex = 0;
        stateCombo.Bind(ComboBox.SelectedItemProperty, new Binding("StateFilter") { Mode = BindingMode.TwoWay });
        controlsRow.Children.Add(new TextBlock { Text = "State:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(gnomeSubtext) });
        controlsRow.Children.Add(stateCombo);

        var sortCombo = new ComboBox
        {
            Width = 120,
            Padding = new Thickness(10, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        sortCombo.Items.Add("UpdatedAt");
        sortCombo.Items.Add("CreatedAt");
        sortCombo.Items.Add("Title");
        sortCombo.SelectedIndex = 0;
        sortCombo.Bind(ComboBox.SelectedItemProperty, new Binding("SortBy") { Mode = BindingMode.TwoWay });
        controlsRow.Children.Add(new TextBlock { Text = "Sort:", VerticalAlignment = VerticalAlignment.Center, FontSize = 12, Foreground = new SolidColorBrush(gnomeSubtext), Margin = new Thickness(12, 0, 0, 0) });
        controlsRow.Children.Add(sortCombo);

        controlsRow.Children.Add(new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch });

        var refreshBtn = new Button { Content = "Refresh", Padding = new Thickness(16, 8), FontSize = 12, CornerRadius = new CornerRadius(6), Background = new SolidColorBrush(gnomeBlue), Foreground = Brushes.White };
        refreshBtn.Bind(Button.CommandProperty, new Binding("RefreshCommand"));
        controlsRow.Children.Add(refreshBtn);

        projIssuesControls.Children.Add(controlsRow);
        DockPanel.SetDock(projIssuesControls, Dock.Top);
        projectIssuesPanelInner.Children.Add(projIssuesControls);

        // ListBox for issues
        var projectIssuesList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        projectIssuesList.Bind(ListBox.ItemsSourceProperty, new Binding("Issues"));
        projectIssuesList.ItemTemplate = CreateIssueTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder, gnomeGreen, gnomeRed);
        projectIssuesPanelInner.Children.Add(projectIssuesList);

        var projectIssuesPanel = new Border
        {
            Child = projectIssuesPanelInner,
            Padding = new Thickness(16)
        };

        var projectIssuesBorder = new Border
        {
            Child = projectIssuesPanel,
            Background = new SolidColorBrush(gnomeSurface),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(gnomeBorder)
        };

        // Search Issues Tab
        var searchPanelInner = new DockPanel { LastChildFill = true };
        
        var searchControls = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };
        searchControls.Children.Add(new TextBlock { Text = "Search issues across all projects:", FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(gnomeText) });

        var searchInputRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var searchQueryBox = new TextBox
        {
            Watermark = "Enter issue title or keywords...",
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
        searchBtn.Bind(Button.CommandProperty, new Binding("SearchIssuesCommand"));
        searchInputRow.Children.Add(searchBtn);

        searchControls.Children.Add(searchInputRow);
        DockPanel.SetDock(searchControls, Dock.Top);
        searchPanelInner.Children.Add(searchControls);

        // ListBox for search results
        var searchResultsList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };
        searchResultsList.Bind(ListBox.ItemsSourceProperty, new Binding("Issues"));
        searchResultsList.ItemTemplate = CreateIssueTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder, gnomeGreen, gnomeRed);
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
        projectIssuesButton.Click += (s, e) =>
        {
            projectIssuesButton.Background = new SolidColorBrush(gnomeBlue);
            projectIssuesButton.Foreground = Brushes.White;
            searchIssuesButton.Background = new SolidColorBrush(gnomeSurface);
            searchIssuesButton.Foreground = new SolidColorBrush(gnomeText);
            projectIssuesBorder.IsVisible = true;
            searchBorder.IsVisible = false;
            if (DataContext is IssuesViewModel vm) vm.ViewMode = "ProjectIssues";
        };

        searchIssuesButton.Click += (s, e) =>
        {
            projectIssuesButton.Background = new SolidColorBrush(gnomeSurface);
            projectIssuesButton.Foreground = new SolidColorBrush(gnomeText);
            searchIssuesButton.Background = new SolidColorBrush(gnomeBlue);
            searchIssuesButton.Foreground = Brushes.White;
            projectIssuesBorder.IsVisible = false;
            searchBorder.IsVisible = true;
            if (DataContext is IssuesViewModel vm) vm.ViewMode = "SearchIssues";
        };

        // Tab container
        var tabContainer = new Grid();
        tabContainer.Children.Add(projectIssuesBorder);
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

    private IDataTemplate CreateIssueTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor, Color successColor, Color errorColor)
    {
        return new FuncDataTemplate<Issue>((issue, _) =>
        {
            var contentStack = new StackPanel { Spacing = 8 };

            // Title and state
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
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(0)
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

            // Labels
            if (issue?.Labels != null && issue.Labels.Length > 0)
            {
                var labelsStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                foreach (var label in issue.Labels.Take(5))
                {
                    var labelBorder = new Border
                    {
                        Child = new TextBlock
                        {
                            Text = label,
                            FontSize = 9,
                            Foreground = new SolidColorBrush(textColor),
                            Margin = new Thickness(6, 2, 6, 2)
                        },
                        Background = new SolidColorBrush(Color.Parse("#E8E8E8")),
                        CornerRadius = new CornerRadius(3),
                        Padding = new Thickness(0)
                    };
                    labelsStack.Children.Add(labelBorder);
                }
                contentStack.Children.Add(labelsStack);
            }

            var infoStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            
            if (issue?.Author != null)
            {
                infoStack.Children.Add(new TextBlock { Text = $"👤 {issue.Author.Name}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            }
            
            infoStack.Children.Add(new TextBlock { Text = $"📅 {GetTimeAgo(issue?.CreatedAt ?? DateTime.Now)}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            
            if (issue?.Upvotes > 0)
            {
                infoStack.Children.Add(new TextBlock { Text = $"👍 {issue.Upvotes}", FontSize = 10, Foreground = new SolidColorBrush(subtextColor) });
            }

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
            _toastService?.ShowToast($"Opening {issue.Title}...", ToastType.Info);
        }
        catch (Exception ex)
        {
            _toastService?.ShowToast($"Failed to open: {ex.Message}", ToastType.Error);
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

