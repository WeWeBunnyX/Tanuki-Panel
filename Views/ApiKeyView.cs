// csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using TanukiPanel.ViewModels;

namespace TanukiPanel.Views;

public class ApiKeyView : UserControl
{
    public TextBox ApiKeyBox { get; } = new TextBox { Width = 320 };

    public ApiKeyView()
    {
        DataContext = new ApiKeyViewModel();

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
        
        ApiKeyBox.Bind(TextBox.TextProperty, new Binding("ApiKey", BindingMode.TwoWay));
        stack.Children.Add(ApiKeyBox);
        var save = new Button { Content = "Save", HorizontalAlignment = HorizontalAlignment.Center };
        
        save.Bind(Button.CommandProperty, new Binding("SaveCommand"));
        stack.Children.Add(save);

        Content = stack;
    }
}