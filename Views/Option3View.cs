using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

public class Option3View : UserControl
{
    public Option3View()
    {
        Content = new Border
        {
            Background = Brushes.White,
            Padding = new Thickness(12),
            Child = new TextBlock { Text = "Option 3 content (placeholder)", FontSize = 16 }
        };
    }
}
