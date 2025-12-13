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
    public TextBox ApiKeyBox { get; }

    public ApiKeyView()
    {
        // DataContext is set by ViewLocator based on the ViewModel

        ApiKeyBox = new TextBox
        {
            Width = 380,
            Height = 40,
            Watermark = "Paste your GitLab API key here",
            FontSize = 13,
            Padding = new Thickness(12, 8),
            BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6)
        };

        var titleBlock = new TextBlock
        {
            Text = "Connect Your GitLab Account",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
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
            Margin = new Thickness(0, 0, 0, 8)
        };

        var subtitleBlock = new TextBlock
        {
            Text = "Provide your GitLab API key to get started",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#999999")),
            Margin = new Thickness(0, 0, 0, 28)
        };

        var labelBlock = new TextBlock
        {
            Text = "API Key",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#333333")),
            Margin = new Thickness(0, 0, 0, 8)
        };

        ApiKeyBox.Bind(TextBox.TextProperty, new Binding("ApiKey", BindingMode.TwoWay));

        var saveButton = new Button
        {
            Content = "Connect & Continue",
            Width = 160,
            Height = 40,
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            CornerRadius = new CornerRadius(6),
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.Parse("#FF6A88"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#FF99AC"), Offset = 1 }
                }
            },
            Foreground = Brushes.White,
            Margin = new Thickness(0, 16, 0, 0)
        };

        saveButton.Bind(Button.CommandProperty, new Binding("SaveCommand"));

        var infoBlock = new TextBlock
        {
            Text = "🔐 Your API key is stored securely on your device",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#CCCCCC")),
            Margin = new Thickness(0, 20, 0, 0)
        };

        var mainStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 0
        };

        mainStack.Children.Add(titleBlock);
        mainStack.Children.Add(subtitleBlock);
        mainStack.Children.Add(labelBlock);
        mainStack.Children.Add(ApiKeyBox);
        mainStack.Children.Add(saveButton);
        mainStack.Children.Add(infoBlock);

        Content = new Grid
        {
            Background = new SolidColorBrush(Color.Parse("#FAFAFA")),
            Children = { mainStack }
        };
    }
}
