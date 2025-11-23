using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

public class Option5View : UserControl
{
    public Option5View()
    {
        Content = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(12),
            Child = new TextBlock { Text = "Option 5 content (placeholder)", FontSize = 16 }
        };
    }
}
