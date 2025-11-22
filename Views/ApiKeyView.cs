using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia;
using Avalonia.Media;

namespace TanukiPanel.Views;

/// <summary>
/// Simple API key entry view: shows instructions, a TextBox and a Save button.
/// </summary>
public class ApiKeyView : UserControl
{
    public TextBox ApiKeyBox { get; } = new TextBox { Width = 320 };

    public ApiKeyView()
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8
        };


        stack.Children.Add(new TextBlock
        {
            Text = "Insert your GitLab API key",
            FontSize = 16,
            TextAlignment = TextAlignment.Center
        });
        

        var label = new TextBlock 
        {
            Text = "API Key:", 
            FontSize = 12 
        };

        stack.Children.Add(label);
        stack.Children.Add(ApiKeyBox);

        var save = new Button { Content = "Save", HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(save);

        Content = stack;
    }
}
