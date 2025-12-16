using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;
using Avalonia.Animation;

namespace TanukiPanel.Views;

/// <summary>
/// A modern sidebar + content view with improved styling and hover effects
/// </summary>
public class SideBarContentView : UserControl
{
    public SideBarContentView()
    {
        // Sidebar with modern gradient and refined styling
        var sidebar = new Border
        {
            Padding = new Thickness(20),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.Parse("#FF6A88"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#FF99AC"), Offset = 1 }
                }
            }
        };

        var logo = new TextBlock
        {
            Text = "🐾 Tanuki",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 4)
        };

        var subtitle = new TextBlock
        {
            Text = "GitLab Panel",
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
            Margin = new Thickness(0, 0, 0, 24),
            Opacity = 0.95
        };

        var divider = new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
            Margin = new Thickness(0, 0, 0, 20)
        };

        var menuStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 10 };

        // Create stylish option buttons with icons
        var buttonOptions = new[]
        {
            ("📊", "Projects", "Projects"),
            ("�", "Container Registry", "Option2"),            ("📥", "Package Registry", "Option3"),            ("📋", "Issues", "Issues"),
            ("🔧", "Settings", "Option4"),
            ("📈", "Analytics", "Option5")
        };

        foreach (var (icon, label, cmd) in buttonOptions)
        {
            var btn = CreateStyledButton($"{icon} {label}", cmd);
            menuStack.Children.Add(btn);
        }

        var sidebarStack = new StackPanel { Orientation = Orientation.Vertical };
        sidebarStack.Children.Add(logo);
        sidebarStack.Children.Add(subtitle);
        sidebarStack.Children.Add(divider);
        sidebarStack.Children.Add(menuStack);

        sidebar.Child = sidebarStack;

        // Content area with modern styling
        var headerStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(20, 16, 20, 12),
            Spacing = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        var headerIcon = new TextBlock
        {
            Text = "▶",
            FontSize = 18,
            Foreground = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.Parse("#FF6A88"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#FF99AC"), Offset = 1 }
                }
            },
            VerticalAlignment = VerticalAlignment.Center
        };

        var header = new TextBlock
        {
            FontSize = 22,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#333333")),
            VerticalAlignment = VerticalAlignment.Center
        };

        header.Bind(TextBlock.TextProperty, new Binding("Title") { Mode = BindingMode.OneWay });
        
        headerStack.Children.Add(headerIcon);
        headerStack.Children.Add(header);

        // Content container with ContentControl for MVVM view resolution
        var contentControl = new ContentControl
        {
            Margin = new Thickness(20, 0, 20, 20),
            Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20)
        };
        
        contentControl.Bind(ContentControl.ContentProperty, new Binding("CurrentViewModel"));
        contentControl.ContentTemplate = new TanukiPanel.ViewLocator();

        var rightStack = new StackPanel { Orientation = Orientation.Vertical };
        rightStack.Children.Add(headerStack);
        rightStack.Children.Add(contentControl);

        // Main grid layout
        var grid = new Grid();
        grid.Background = new SolidColorBrush(Color.Parse("#FAFAFA"));
        grid.ColumnDefinitions = new ColumnDefinitions("260, *");

        Grid.SetColumn(sidebar, 0);
        Grid.SetColumn(rightStack, 1);

        grid.Children.Add(sidebar);
        grid.Children.Add(rightStack);

        // Bind buttons to SelectCommand
        int idx = 0;
        foreach (var child in menuStack.Children)
        {
            if (child is Button btn)
            {
                btn.Bind(Button.CommandProperty, new Binding("SelectCommand"));
                btn.CommandParameter = buttonOptions[idx].Item3;
                idx++;
            }
        }

        Content = grid;
    }

    /// <summary>
    /// Creates a styled button with hover effects
    /// </summary>
    private Button CreateStyledButton(string content, string commandParam)
    {
        var button = new Button
        {
            Content = content,
            Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(14, 12),
            CornerRadius = new CornerRadius(8),
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };

        // Add hover effect via pointer events
        button.PointerEntered += (s, e) =>
        {
            if (s is Button btn)
            {
                btn.Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));
            }
        };

        button.PointerExited += (s, e) =>
        {
            if (s is Button btn)
            {
                btn.Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
            }
        };

        return button;
    }
}

