using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using TanukiPanel.Models;
using TanukiPanel.ViewModels;
using TanukiPanel.Views.Converters;

namespace TanukiPanel.Views;

public class CommitView : UserControl
{
    public CommitView()
    {
        var gnomeBlue = Color.Parse("#3584E4");
        var gnomeBackground = Color.Parse("#F6F5F4");
        var gnomeSurface = Color.Parse("#FFFFFF");
        var gnomeText = Color.Parse("#2E2E2E");
        var gnomeBorder = Color.Parse("#CCCCCC");
        var gnomeSubtext = Color.Parse("#77767B");
        var gnomePurple = Color.Parse("#9141AC");

        // Main Grid instead of DockPanel
        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header + controls
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content (fills remaining space)
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Status bar

        // Header and controls section (all in row 0)
        var controlsSection = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 16)
        };

        // Header
        controlsSection.Children.Add(new TextBlock
        {
            Text = "📝 Commit Viewer",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 8)
        });

        controlsSection.Children.Add(new TextBlock
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

        var loadBtn = new Button
        {
            Content = "Load Repo",
            Padding = new Thickness(20, 10),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        loadBtn.Bind(Button.CommandProperty, new Binding("LoadRepositoryCommand"));
        repoInputRow.Children.Add(loadBtn);
        controlsSection.Children.Add(repoInputRow);

        // Date filter section
        var dateFilterSection = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F0E6FF")),
            BorderBrush = new SolidColorBrush(gnomePurple),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var dateFilterStack = new StackPanel { Spacing = 12 };

        var filterCheckBox = new CheckBox
        {
            Content = "Filter by date range",
            FontSize = 12,
            Foreground = new SolidColorBrush(gnomeText)
        };
        filterCheckBox.Bind(CheckBox.IsCheckedProperty, new Binding("FilterByDate") { Mode = BindingMode.TwoWay });
        dateFilterStack.Children.Add(filterCheckBox);

        var dateInputRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };

        dateInputRow.Children.Add(new TextBlock
        {
            Text = "From:",
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeText),
            VerticalAlignment = VerticalAlignment.Center
        });

        var startDateBox = new TextBox
        {
            Width = 120,
            Padding = new Thickness(8, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1)
        };
        startDateBox.Bind(TextBox.TextProperty, new Binding("StartDate") { StringFormat = "yyyy-MM-dd", Mode = BindingMode.TwoWay });
        dateInputRow.Children.Add(startDateBox);

        dateInputRow.Children.Add(new TextBlock
        {
            Text = "To:",
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeText),
            VerticalAlignment = VerticalAlignment.Center
        });

        var endDateBox = new TextBox
        {
            Width = 120,
            Padding = new Thickness(8, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1)
        };
        endDateBox.Bind(TextBox.TextProperty, new Binding("EndDate") { StringFormat = "yyyy-MM-dd", Mode = BindingMode.TwoWay });
        dateInputRow.Children.Add(endDateBox);

        var fetchBtn = new Button
        {
            Content = "🔄 Refresh",
            Padding = new Thickness(16, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(gnomePurple),
            Foreground = Brushes.White,
            Margin = new Thickness(12, 0, 0, 0)
        };
        fetchBtn.Bind(Button.CommandProperty, new Binding("FetchCommitsCommand"));
        dateInputRow.Children.Add(fetchBtn);

        dateFilterStack.Children.Add(dateInputRow);
        dateFilterSection.Child = dateFilterStack;
        controlsSection.Children.Add(dateFilterSection);

        Grid.SetRow(controlsSection, 0);
        mainGrid.Children.Add(controlsSection);

        // Commits list with scrolling (row 1 - fills remaining space)
        var commitsList = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            MaxHeight = 400
        };
        commitsList.ItemTemplate = CreateCommitTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBorder, gnomePurple);
        commitsList.Bind(ListBox.ItemsSourceProperty, new Binding("Commits"));

        Grid.SetRow(commitsList, 1);
        mainGrid.Children.Add(commitsList);

        // Pagination controls (row 2) - moved here before status bar
        var paginationStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 12, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var prevBtn = new Button
        {
            Content = "← Previous",
            Padding = new Thickness(12, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        prevBtn.Bind(Button.CommandProperty, new Binding("PreviousPageCommand"));
        prevBtn.Bind(Button.IsEnabledProperty, new Binding("HasPreviousPage"));
        paginationStack.Children.Add(prevBtn);

        var pageInfoBlock = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeSubtext),
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 200
        };
        pageInfoBlock.Bind(TextBlock.TextProperty, new Binding("PageInfo"));
        paginationStack.Children.Add(pageInfoBlock);

        var nextBtn = new Button
        {
            Content = "Next →",
            Padding = new Thickness(12, 8),
            FontSize = 11,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        nextBtn.Bind(Button.CommandProperty, new Binding("NextPageCommand"));
        nextBtn.Bind(Button.IsEnabledProperty, new Binding("HasNextPage"));
        paginationStack.Children.Add(nextBtn);

        // Status bar
        var statusBar = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeSubtext),
            Margin = new Thickness(0, 12, 0, 0)
        };
        statusBar.Bind(TextBlock.TextProperty, new Binding("LoadingMessage"));

        // Combine pagination and status in a single row 2
        var bottomStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };
        bottomStack.Children.Add(paginationStack);
        bottomStack.Children.Add(statusBar);

        Grid.SetRow(bottomStack, 2);
        mainGrid.Children.Add(bottomStack);

        Content = new Border
        {
            Child = mainGrid,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24)
        };
    }

    private IDataTemplate CreateCommitTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color borderColor, Color accentColor)
    {
        return new FuncDataTemplate<Commit>((commit, _) =>
        {
            var contentStack = new StackPanel { Spacing = 6 };

            // Commit hash and title
            var titleStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            titleStack.Children.Add(new TextBlock
            {
                Text = commit?.ShortId ?? "unknown",
                FontSize = 10,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(accentColor),
                FontFamily = "Courier New",
                MinWidth = 60
            });

            titleStack.Children.Add(new TextBlock
            {
                Text = commit?.Title ?? "No title",
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor),
                TextWrapping = TextWrapping.Wrap
            });
            contentStack.Children.Add(titleStack);

            // Author and date info
            var infoStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16 };
            infoStack.Children.Add(new TextBlock
            {
                Text = $"👤 {commit?.AuthorName ?? "Unknown"}",
                FontSize = 10,
                Foreground = new SolidColorBrush(subtextColor)
            });

            infoStack.Children.Add(new TextBlock
            {
                Text = $"📅 {GetTimeAgo(commit?.CreatedAt ?? DateTime.Now)}",
                FontSize = 10,
                Foreground = new SolidColorBrush(subtextColor)
            });
            contentStack.Children.Add(infoStack);

            // Full message (if different from title)
            if (!string.IsNullOrWhiteSpace(commit?.Message) && commit.Message != commit?.Title)
            {
                var messageBlock = new TextBlock
                {
                    Text = commit.Message.Split('\n')[0],
                    FontSize = 10,
                    Foreground = new SolidColorBrush(subtextColor),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                contentStack.Children.Add(messageBlock);
            }

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
