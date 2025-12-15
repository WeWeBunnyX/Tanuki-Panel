using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using TanukiPanel.Models;
using TanukiPanel.ViewModels;

namespace TanukiPanel.Views;

public class Option1View : UserControl
{
    public Option1View()
    {
        // Header
        var headerBlock = new TextBlock
        {
            Text = "📊 Your GitLab Projects",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#333333")),
            Margin = new Thickness(0, 0, 0, 16)
        };

        // Loading indicator
        var loadingStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        var loadingText = new TextBlock
        {
            Text = "⏳ Loading...",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.Parse("#999999")),
            TextAlignment = TextAlignment.Center
        };

        var loadingMessage = new TextBlock
        {
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#CCCCCC")),
            TextAlignment = TextAlignment.Center
        };
        loadingMessage.Bind(TextBlock.TextProperty, new Binding("LoadingMessage"));

        loadingStack.Children.Add(loadingText);
        loadingStack.Children.Add(loadingMessage);

        var loadingBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F9F9F9")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Child = loadingStack
        };

        // Refresh button
        var refreshButton = new Button
        {
            Content = "🔄 Refresh Projects",
            Padding = new Thickness(12, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.Parse("#FF6A88"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#FF99AC"), Offset = 1 }
                }
            },
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 16)
        };
        refreshButton.Bind(Button.CommandProperty, new Binding("RefreshCommand"));

        // Projects ScrollViewer
        var projectsList = new ItemsControl();
        projectsList.Bind(ItemsControl.ItemsSourceProperty, new Binding("Projects"));
        projectsList.ItemTemplate = CreateProjectTemplate();

        var scrollViewer = new ScrollViewer
        {
            Content = projectsList,
            Padding = new Thickness(0, 0, 8, 0)
        };

        // Main stack
        var mainStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        mainStack.Children.Add(headerBlock);
        mainStack.Children.Add(refreshButton);

        // Container for loading vs content
        var contentPresenter = new Grid();
        contentPresenter.Children.Add(scrollViewer);
        contentPresenter.Children.Add(loadingBorder);

        // Bind loading indicator visibility
        var loadingBinding = new Binding("IsLoading");
        loadingBorder.Bind(IsVisibleProperty, loadingBinding);

        mainStack.Children.Add(contentPresenter);

        Content = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(16),
            Child = mainStack
        };
    }

    private IDataTemplate CreateProjectTemplate()
    {
        return new FuncDataTemplate<Models.Project>((project, _) =>
        {
            var nameBlock = new TextBlock
            {
                Text = project?.Name ?? "Unknown",
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.Parse("#333333")),
                TextWrapping = TextWrapping.Wrap
            };

            var descBlock = new TextBlock
            {
                Text = project?.Description ?? "No description",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.Parse("#999999")),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 8)
            };

            var statsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16
            };

            if (project != null)
            {
                var starText = new TextBlock
                {
                    Text = $"⭐ {project.StarCount}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.Parse("#FF9800"))
                };

                var forkText = new TextBlock
                {
                    Text = $"🔀 {project.ForksCount}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.Parse("#2196F3"))
                };

                var issueText = new TextBlock
                {
                    Text = $"📋 {project.OpenIssuesCount}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.Parse("#4CAF50"))
                };

                var visibilityText = new TextBlock
                {
                    Text = $"🔒 {project.Visibility}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.Parse("#666666"))
                };

                statsStack.Children.Add(starText);
                statsStack.Children.Add(forkText);
                statsStack.Children.Add(issueText);
                statsStack.Children.Add(visibilityText);
            }

            var contentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 0
            };

            contentStack.Children.Add(nameBlock);
            contentStack.Children.Add(descBlock);
            contentStack.Children.Add(statsStack);

            var openButton = new Button
            {
                Content = "Open in GitLab ↗",
                Padding = new Thickness(8, 4),
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.Parse("#F0F0F0")),
                Foreground = new SolidColorBrush(Color.Parse("#333333"))
            };

            if (project != null)
            {
                openButton.Click += (s, e) =>
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = project.WebUrl,
                            UseShellExecute = true
                        });
                    }
                    catch { }
                };
            }

            var projectStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 0
            };

            projectStack.Children.Add(contentStack);
            projectStack.Children.Add(openButton);

            var projectBorder = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FAFAFA")),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                Child = projectStack
            };

            return projectBorder;
        });
    }
}
