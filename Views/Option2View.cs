using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

public class Option2View : UserControl
{
    public Option2View()
    {
        Content = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(12),
            Child = new TextBlock { Text = "Option 2 content (placeholder)", FontSize = 16 }
        };
    }
}
