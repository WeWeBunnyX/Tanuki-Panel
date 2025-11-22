using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

public class InsertApiScreen : UserControl
{
    public InsertApiScreen()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(6)
        };

        stack.Children.Add(new Button { Content = "Button 1", Margin = new Avalonia.Thickness(4) });
        stack.Children.Add(new Button { Content = "Button 2", Margin = new Avalonia.Thickness(4) });

        Content = new Border
        {
            Background = Brushes.LightGray,
            Child = stack
        };
    }
}