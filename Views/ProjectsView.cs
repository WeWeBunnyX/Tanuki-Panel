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
        // Get toast service from DI container
        var app = Application.Current as App;
        _toastService = app?.ServiceProvider?.GetService<IToastService>();

        // GNOME color scheme
        var gnomeBlue = Color.Parse("#3584E4");
        var gnomeBackground = Color.Parse("#F6F5F4");
        var gnomeSurface = Color.Parse("#FFFFFF");
        var gnomeText = Color.Parse("#2E2E2E");
        var gnomeBorder = Color.Parse("#CCCCCC");
        var gnomeSubtext = Color.Parse("#77767B");

        // Header
        var headerBlock = new TextBlock
        {
            Text = "Projects",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 24)
        };

        // Search box with GNOME styling
        var searchBox = new TextBox
        {
            Watermark = "Search projects...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 16),
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        searchBox.Bind(TextBox.TextProperty, new Binding("SearchText") { Mode = BindingMode.TwoWay });

        // Controls row
        var controlsRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 16)
        };

        // Visibility filter with GNOME styling
        var visibilityLabel = new TextBlock
        {
            Text = "Visibility:",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            Foreground = new SolidColorBrush(gnomeSubtext)
        };

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

        // Sort dropdown with GNOME styling
        var sortLabel = new TextBlock
        {
            Text = "Sort by:",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            Foreground = new SolidColorBrush(gnomeSubtext),
            Margin = new Thickness(12, 0, 0, 0)
        };

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

        // Hide archived toggle
        var hideArchivedCheck = new CheckBox
        {
            Content = "Hide Archived",
            Padding = new Thickness(8),
            FontSize = 12,
            IsChecked = true,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(12, 0, 0, 0)
        };
        hideArchivedCheck.Bind(CheckBox.IsCheckedProperty, new Binding("HideArchived") { Mode = BindingMode.TwoWay });

        // Refresh button with GNOME styling
        var refreshButton = new Button
        {
            Content = "Refresh",
            Padding = new Thickness(16, 8),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White,
            Margin = new Thickness(12, 0, 0, 0)
        };
        refreshButton.Bind(Button.CommandProperty, new Binding("RefreshCommand"));

        controlsRow.Children.Add(visibilityLabel);
        controlsRow.Children.Add(visibilityCombo);
        controlsRow.Children.Add(sortLabel);
        controlsRow.Children.Add(sortCombo);
        controlsRow.Children.Add(hideArchivedCheck);
        controlsRow.Children.Add(new StackPanel { HorizontalAlignment = HorizontalAlignment.Stretch }); // Spacer
        controlsRow.Children.Add(refreshButton);

        // Loading indicator
        var loadingStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        var loadingText = new TextBlock
        {
            Text = "Loading projects...",
            FontSize = 14,
            Foreground = new SolidColorBrush(gnomeText),
            TextAlignment = TextAlignment.Center
        };

        var loadingMessage = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeSubtext),
            TextAlignment = TextAlignment.Center
        };
        loadingMessage.Bind(TextBlock.TextProperty, new Binding("LoadingMessage"));

        loadingStack.Children.Add(loadingText);
        loadingStack.Children.Add(loadingMessage);

        var loadingBorder = new Border
        {
            Background = new SolidColorBrush(gnomeBackground),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(32),
            Child = loadingStack
        };

        // Projects list
        var projectsList = new ItemsControl();
        projectsList.Bind(ItemsControl.ItemsSourceProperty, new Binding("Projects"));
        projectsList.ItemTemplate = CreateProjectTemplate(gnomeText, gnomeSubtext, gnomeSurface, gnomeBlue, gnomeBorder);

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
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24),
            Child = mainStack
        };
    }

    private IDataTemplate CreateProjectTemplate(Color textColor, Color subtextColor, Color surfaceColor, Color accentColor, Color borderColor)
    {
        return new FuncDataTemplate<Models.Project>((project, _) =>
        {
            var nameBlock = new TextBlock
            {
                Text = project?.Name ?? "Unknown",
                FontSize = 15,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(textColor),
                TextWrapping = TextWrapping.Wrap
            };

            var descBlock = new TextBlock
            {
                Text = project?.Description ?? "No description",
                FontSize = 12,
                Foreground = new SolidColorBrush(subtextColor),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 6, 0, 12)
            };

            // Activity timestamp
            var activityBlock = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(subtextColor),
                Margin = new Thickness(0, 0, 0, 12)
            };
            if (project != null)
            {
                var timeAgo = GetTimeAgo(project.LastActivityAt);
                activityBlock.Text = $"Last activity: {timeAgo}";
            }

            // Stats row
            var statsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 20,
                Margin = new Thickness(0, 0, 0, 12)
            };

            if (project != null)
            {
                var starText = new TextBlock
                {
                    Text = $"⭐ {project.StarCount}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(subtextColor)
                };

                var forkText = new TextBlock
                {
                    Text = $"🔀 {project.ForksCount}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(subtextColor)
                };

                var issueText = new TextBlock
                {
                    Text = $"📋 {project.OpenIssuesCount}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(subtextColor)
                };

                var visibilityText = new TextBlock
                {
                    Text = project.Visibility,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(subtextColor),
                    FontStyle = FontStyle.Italic
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
                Spacing = 10,
                Margin = new Thickness(0, 12, 0, 0)
            };

            var openButton = new Button
            {
                Content = "Open",
                Padding = new Thickness(14, 8),
                FontSize = 11,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(accentColor),
                Foreground = Brushes.White
            };

            var copyButton = new Button
            {
                Content = "Copy URL",
                Padding = new Thickness(14, 8),
                FontSize = 11,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(surfaceColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                Foreground = new SolidColorBrush(textColor)
            };

            if (project != null)
            {
                openButton.Click += (s, e) =>
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(project.WebUrl))
                        {
                            _toastService?.ShowToast("Error: Project URL is empty", ToastType.Error);
                            return;
                        }

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
                        
                        _toastService?.ShowToast($"Opening {project.Name}...", ToastType.Info);
                    }
                    catch (Exception ex)
                    {
                        _toastService?.ShowToast($"Failed to open: {ex.Message}", ToastType.Error);
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

                        _toastService?.ShowToast($"Copied: {url}", ToastType.Success);
                    }
                    catch (Exception ex)
                    {
                        _toastService?.ShowToast($"Failed to copy: {ex.Message}", ToastType.Error);
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
                Background = new SolidColorBrush(surfaceColor),
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12),
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

