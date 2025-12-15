using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using TanukiPanel.Models;
using TanukiPanel.ViewModels;

namespace TanukiPanel.Views;

public class ProjectsView : UserControl
{
    public ProjectsView()
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

        // Search box
        var searchBox = new TextBox
        {
            Watermark = "🔍 Search by name or description...",
            Padding = new Thickness(12, 8),
            FontSize = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };
        searchBox.Bind(TextBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });

        // Controls row (filters, sort, refresh)
        var controlsRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 12)
        };

        // Visibility filter
        var visibilityCombo = new ComboBox
        {
            Width = 120,
            Padding = new Thickness(8),
            FontSize = 11
        };
        visibilityCombo.Items.Add("All");
        visibilityCombo.Items.Add("public");
        visibilityCombo.Items.Add("private");
        visibilityCombo.Items.Add("internal");
        visibilityCombo.SelectedIndex = 0;
        visibilityCombo.Bind(ComboBox.SelectedItemProperty, new Binding("FilterVisibility") { Mode = BindingMode.TwoWay });

        // Sort dropdown
        var sortCombo = new ComboBox
        {
            Width = 130,
            Padding = new Thickness(8),
            FontSize = 11
        };
        sortCombo.Items.Add("LastActivity");
        sortCombo.Items.Add("Name");
        sortCombo.Items.Add("Stars");
        sortCombo.SelectedIndex = 0;
        sortCombo.Bind(ComboBox.SelectedItemProperty, new Binding("SortBy") { Mode = BindingMode.TwoWay });

        // Hide archived toggle
        var hideArchivedCheck = new CheckBox
        {
            Content = "📦 Hide Archived",
            Padding = new Thickness(4),
            FontSize = 11,
            IsChecked = true
        };
        hideArchivedCheck.Bind(CheckBox.IsCheckedProperty, new Binding("HideArchived") { Mode = BindingMode.TwoWay });

        // Refresh button
        var refreshButton = new Button
        {
            Content = "🔄 Refresh",
            Padding = new Thickness(12, 6),
            FontSize = 11,
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
            Foreground = Brushes.White
        };
        refreshButton.Bind(Button.CommandProperty, new Binding("RefreshCommand"));

        controlsRow.Children.Add(new TextBlock { Text = "Filter:", VerticalAlignment = VerticalAlignment.Center, FontSize = 11 });
        controlsRow.Children.Add(visibilityCombo);
        controlsRow.Children.Add(new TextBlock { Text = "Sort:", VerticalAlignment = VerticalAlignment.Center, FontSize = 11, Margin = new Thickness(8, 0, 0, 0) });
        controlsRow.Children.Add(sortCombo);
        controlsRow.Children.Add(hideArchivedCheck);
        controlsRow.Children.Add(refreshButton);

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

        // Projects list
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
        mainStack.Children.Add(searchBox);
        mainStack.Children.Add(controlsRow);

        // Container for loading vs content
        var contentPresenter = new Grid();
        contentPresenter.Children.Add(scrollViewer);
        contentPresenter.Children.Add(loadingBorder);

        // Bind loading indicator visibility
        loadingBorder.Bind(IsVisibleProperty, new Binding("IsLoading"));

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

            // Activity timestamp
            var activityBlock = new TextBlock
            {
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.Parse("#AAAAAA")),
                Margin = new Thickness(0, 0, 0, 8)
            };
            if (project != null)
            {
                var timeAgo = GetTimeAgo(project.LastActivityAt);
                activityBlock.Text = $"Last activity: {timeAgo}";
            }

            var statsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16,
                Margin = new Thickness(0, 0, 0, 8)
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

            // Buttons row
            var buttonsRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var openButton = new Button
            {
                Content = "🌐 Open in GitLab",
                Padding = new Thickness(10, 6),
                FontSize = 10,
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
                Foreground = Brushes.White
            };

            var copyButton = new Button
            {
                Content = "📋 Copy Clone",
                Padding = new Thickness(10, 6),
                FontSize = 10,
                Background = new SolidColorBrush(Color.Parse("#E8E8E8")),
                Foreground = new SolidColorBrush(Color.Parse("#333333"))
            };

            if (project != null)
            {
                openButton.Click += (s, e) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(project.WebUrl))
                        {
                            System.Console.WriteLine("Error: Project URL is empty");
                            return;
                        }

                        System.Console.WriteLine($"Opening: {project.WebUrl}");

                        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = project.WebUrl,
                                UseShellExecute = true
                            };
                            System.Diagnostics.Process.Start(psi);
                        }
                        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "open",
                                Arguments = project.WebUrl,
                                UseShellExecute = true
                            };
                            System.Diagnostics.Process.Start(psi);
                        }
                        else
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "xdg-open",
                                Arguments = $"\"{project.WebUrl}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            var proc = System.Diagnostics.Process.Start(psi);
                            proc?.WaitForExit(2000);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Failed to open: {ex.Message}");
                    }
                };

                copyButton.Click += (s, e) =>
                {
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

                        System.Console.WriteLine($"Copied: {url}");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Copy failed: {ex.Message}");
                    }
                };
            }

            buttonsRow.Children.Add(openButton);
            buttonsRow.Children.Add(copyButton);

            var contentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 0
            };

            contentStack.Children.Add(nameBlock);
            contentStack.Children.Add(descBlock);
            contentStack.Children.Add(activityBlock);
            contentStack.Children.Add(statsStack);
            contentStack.Children.Add(buttonsRow);

            var projectBorder = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#FAFAFA")),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                Child = contentStack
            };

            return projectBorder;
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

