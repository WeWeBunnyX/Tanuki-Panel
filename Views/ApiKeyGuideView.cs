using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace TanukiPanel.Views;

/// <summary>
/// View displaying a guide with snapshots on how to find the API key.
/// Features swipeable carousel navigation between snapshots.
/// </summary>
public class ApiKeyGuideView : UserControl
{
    private int _currentSnapshotIndex = 0;
    private double _startX = 0;
    private bool _isResettingPosition = false;

    public ApiKeyGuideView()
    {
        var guideColor = Color.Parse("#F6F5F4");
        var textColor = Color.Parse("#2E2E2E");
        var blueColor = Color.Parse("#3584E4");
        var borderColor = Color.Parse("#D0CFCC");

        // Create the header with back button
        var backButton = new Button
        {
            Content = "← Back",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(16, 16, 0, 0),
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidColorBrush(blueColor),
            Foreground = new SolidColorBrush(Color.Parse("#FFFFFF")),
            FontSize = 14,
            ZIndex = 10
        };

        // Bind the back button to the command
        backButton.Click += (s, e) =>
        {
            if (DataContext is ViewModels.ApiKeyGuideViewModel vm)
            {
                vm.GoBackCommand.Execute(null);
            }
        };

        // Create the title
        var titleText = new TextBlock
        {
            Text = "How to Find Your API Key",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(textColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 16, 0, 24)
        };

        // Create carousel container
        var carouselContainer = new Border
        {
            BorderBrush = new SolidColorBrush(borderColor),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Width = 600,
            Height = 400,
            Margin = new Thickness(0, 0, 0, 24),
            ClipToBounds = true
        };

        // Create carousel panel for snapshots
        var carouselPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        // Create snapshot items
        var snapshotItems = new Border[3];
        for (int i = 0; i < 3; i++)
        {
            var snapshotContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 12,
                Width = 600,
                Height = 400
            };

            snapshotContent.Children.Add(new TextBlock
            {
                Text = $"Snapshot {i + 1}",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(blueColor),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            snapshotContent.Children.Add(new TextBlock
            {
                Text = "Add your image here",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#77767B")),
                HorizontalAlignment = HorizontalAlignment.Center
            });

            var snapshotItem = new Border
            {
                Width = 600,
                Height = 400,
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                Child = snapshotContent
            };

            snapshotItems[i] = snapshotItem;
            carouselPanel.Children.Add(snapshotItem);
        }

        // Add carousel panel to container
        var carouselScroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = carouselPanel,
            ClipToBounds = true
        };

        carouselContainer.Child = carouselScroll;

        // Create indicator dots
        var dotsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 24)
        };

        var dots = new Button[3];
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            var dot = new Button
            {
                Width = 12,
                Height = 12,
                Padding = new Thickness(0),
                Background = i == 0 ? new SolidColorBrush(blueColor) : new SolidColorBrush(Color.Parse("#D0CFCC")),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6)
            };

            dot.Click += (s, e) => {
                GoToSnapshot(index, carouselPanel, dots);
            };

            dots[i] = dot;
            dotsPanel.Children.Add(dot);
        }

        // Add swipe gesture handling
        carouselContainer.PointerPressed += (s, e) =>
        {
            _startX = e.GetPosition(carouselContainer).X;
        };

        carouselContainer.PointerReleased += (s, e) =>
        {
            double endX = e.GetPosition(carouselContainer).X;
            double distance = endX - _startX;

            if (System.Math.Abs(distance) > 50) // Minimum swipe distance
            {
                if (distance > 0 && _currentSnapshotIndex > 0)
                {
                    _currentSnapshotIndex--;
                }
                else if (distance < 0 && _currentSnapshotIndex < 2)
                {
                    _currentSnapshotIndex++;
                }

                GoToSnapshot(_currentSnapshotIndex, carouselPanel, dots);
            }
        };

        // Create the main content stack
        var contentStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Spacing = 0,
            Margin = new Thickness(0, 60, 0, 0)
        };

        contentStack.Children.Add(titleText);
        contentStack.Children.Add(carouselContainer);
        contentStack.Children.Add(dotsPanel);

        // Create the grid with back button overlaid
        var grid = new Grid
        {
            Background = new SolidColorBrush(guideColor),
            RowDefinitions = new RowDefinitions("Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };

        // Add children to grid
        Grid.SetRow(backButton, 0);
        Grid.SetColumn(backButton, 0);
        grid.Children.Add(backButton);

        Grid.SetRow(contentStack, 0);
        Grid.SetRowSpan(contentStack, 2);
        Grid.SetColumn(contentStack, 0);
        grid.Children.Add(contentStack);

        Content = grid;
    }

    private void GoToSnapshot(int index, StackPanel carouselPanel, Button[] dots)
    {
        _currentSnapshotIndex = index;
        
        // Update carousel position
        double offset = -index * 600;
        carouselPanel.RenderTransform = new TranslateTransform(offset, 0);

        // Update dots
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].Background = i == index 
                ? new SolidColorBrush(Color.Parse("#3584E4")) 
                : new SolidColorBrush(Color.Parse("#D0CFCC"));
        }
    }
}

