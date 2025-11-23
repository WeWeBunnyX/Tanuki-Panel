using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;

namespace TanukiPanel.Views;

/// <summary>
/// A sidebar + content view 
/// </summary>
public class SideBarContentView : UserControl
{
    public SideBarContentView()
    {
        //Sidebar
        var sidebar = new Border
        {
            Padding = new Thickness(16),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0,0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1,1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.Parse("#FF6A88"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#FF99AC"), Offset = 1 }
                }
            },
            CornerRadius = new CornerRadius(8)
        };

        var logo = new TextBlock
        {
            Text = "Tanuki",
            FontSize = 22,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var subtitle = new TextBlock
        {
            Text = "Dashboard",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
            Margin = new Thickness(0, 0, 0, 18)
        };

        var menuStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 8 };

        var opt1 = new Button
        {
            Content = "Option 1",
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(6)
        };

        var opt2 = new Button
        {
            Content = "Option 2",
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(6)
        };

        var opt3 = new Button
        {
            Content = "Option 3",
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(6)
        };

        var opt4 = new Button
        {
            Content = "Option 4",
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(6)
        };

        var opt5 = new Button
        {
            Content = "Option 5",
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(6)
        };

        menuStack.Children.Add(opt1);
        menuStack.Children.Add(opt2);
        menuStack.Children.Add(opt3);
        menuStack.Children.Add(opt4);
        menuStack.Children.Add(opt5);

        var sidebarStack = new StackPanel { Orientation = Orientation.Vertical };
        sidebarStack.Children.Add(logo);
        sidebarStack.Children.Add(subtitle);
        sidebarStack.Children.Add(menuStack);

        sidebar.Child = sidebarStack;

        //Content area
        var header = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(12, 6),
            Foreground = Brushes.Black
        };


        header.Bind(TextBlock.TextProperty, new Binding("Title") { Mode = BindingMode.OneWay });

        var contentBox = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F7F9FC")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Margin = new Thickness(12)
        };

        var contentText = new TextBlock
        {
            Text = "Select an option from the sidebar to begin.",
            FontSize = 14,
            Foreground = Brushes.DarkGray
        };

        contentBox.Child = contentText;

        var rightStack = new StackPanel { Orientation = Orientation.Vertical };
        rightStack.Children.Add(header);
        rightStack.Children.Add(contentBox);



        var grid = new Grid();
        grid.ColumnDefinitions = new ColumnDefinitions("240, *");

        Grid.SetColumn(sidebar, 0);
        Grid.SetColumn(rightStack, 1);

    grid.Children.Add(sidebar);
    grid.Children.Add(rightStack);

    opt1.Click += (_, _) => contentBox.Child = new Option1View();
    opt2.Click += (_, _) => contentBox.Child = new Option2View();
    opt3.Click += (_, _) => contentBox.Child = new Option3View();
    opt4.Click += (_, _) => contentBox.Child = new Option4View();
    opt5.Click += (_, _) => contentBox.Child = new Option5View();

        Content = grid;
    }
}
