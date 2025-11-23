using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

public class Option4View : UserControl
{
    public Option4View()
    {
        Content = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(12),
            Child = new TextBlock { Text = "Option 4 content (placeholder)", FontSize = 16 }
        };
    }
}
