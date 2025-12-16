using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Data;

namespace TanukiPanel.Views;

public class ContainerRegistryView : UserControl
{
    public ContainerRegistryView()
    {
        var gnomeBlue = Color.Parse("#3584E4");
        var gnomeBackground = Color.Parse("#F6F5F4");
        var gnomeSurface = Color.Parse("#FFFFFF");
        var gnomeText = Color.Parse("#2E2E2E");
        var gnomeBorder = Color.Parse("#CCCCCC");
        var gnomeSubtext = Color.Parse("#77767B");
        var gnomeGreen = Color.Parse("#33D17A");

        // Main grid layout
        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Repository input
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content

        // Header
        var headerBlock = new TextBlock
        {
            Text = "Container Registry",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 16)
        };
        Grid.SetRow(headerBlock, 0);
        mainGrid.Children.Add(headerBlock);

        // Repository input section
        var repoSection = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 16)
        };

        repoSection.Children.Add(new TextBlock
        {
            Text = "Select Repository",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeText)
        });

        var repoInputRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var repoInput = new TextBox
        {
            Watermark = "Paste repository URL or path (e.g., group/project)...",
            Padding = new Thickness(12, 10),
            FontSize = 13,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(gnomeSurface),
            BorderBrush = new SolidColorBrush(gnomeBorder),
            BorderThickness = new Thickness(1),
            Foreground = new SolidColorBrush(gnomeText)
        };
        repoInputRow.Children.Add(repoInput);

        var selectBtn = new Button
        {
            Content = "Select",
            Padding = new Thickness(20, 10),
            FontSize = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White
        };
        repoInputRow.Children.Add(selectBtn);

        repoSection.Children.Add(repoInputRow);
        Grid.SetRow(repoSection, 1);
        mainGrid.Children.Add(repoSection);

        // Content area with upload and registry list
        var contentGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Upload section
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Registry list

        // Upload section
        var uploadSection = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#E8F4FD")),
            BorderBrush = new SolidColorBrush(Color.Parse("#B3D9F2")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 16)
        };

        var uploadStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        uploadStack.Children.Add(new TextBlock
        {
            Text = "📤 Drop files here or click to upload",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeBlue),
            TextAlignment = TextAlignment.Center
        });

        uploadStack.Children.Add(new TextBlock
        {
            Text = "Supported: Docker images, OCI artifacts, and other container formats",
            FontSize = 11,
            Foreground = new SolidColorBrush(gnomeSubtext),
            TextAlignment = TextAlignment.Center
        });

        var uploadBtn = new Button
        {
            Content = "Select Files to Upload",
            Padding = new Thickness(24, 12),
            FontSize = 13,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        uploadStack.Children.Add(uploadBtn);

        uploadSection.Child = uploadStack;
        Grid.SetRow(uploadSection, 0);
        contentGrid.Children.Add(uploadSection);

        // Registry items list
        var registryListSection = new Grid
        {
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        registryListSection.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        registryListSection.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        registryListSection.Children.Add(new TextBlock
        {
            Text = "Registry Images",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(gnomeText),
            Margin = new Thickness(0, 0, 0, 12)
        });

        // Placeholder list
        var listBox = new ListBox
        {
            Padding = new Thickness(0),
            BorderThickness = new Thickness(0)
        };

        var scrollViewer = new ScrollViewer
        {
            Content = listBox,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Grid.SetRow(scrollViewer, 1);
        registryListSection.Children.Add(scrollViewer);

        Grid.SetRow(registryListSection, 1);
        contentGrid.Children.Add(registryListSection);

        Grid.SetRow(contentGrid, 2);
        mainGrid.Children.Add(contentGrid);

        Content = new Border
        {
            Child = mainGrid,
            Background = new SolidColorBrush(gnomeBackground),
            Padding = new Thickness(24)
        };
    }
}

