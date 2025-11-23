using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

public class Option1View : UserControl
{
    public Option1View()
    {
        Content = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(12),
            Child = new TextBlock { Text = "Option 1 content (placeholder)", FontSize = 16 }
        };
    }
}
