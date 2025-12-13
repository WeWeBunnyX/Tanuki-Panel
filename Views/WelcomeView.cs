using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Animation;
using Avalonia.Media;

namespace TanukiPanel.Views;

/// <summary>
/// A welcome view with modern styling that fades in a message, waits, then fades out and signals completion.
/// </summary>
public class WelcomeView : UserControl
{
    private readonly StackPanel _contentStack;
    private readonly TextBlock _mainText;
    private readonly TextBlock _subtitleText;
    private readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(800);

    public WelcomeView()
    {
        _mainText = new TextBlock
        {
            Text = "Tanuki Panel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 48,
            FontWeight = FontWeight.Bold,
            Opacity = 0,
            Foreground = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop { Color = Color.Parse("#FF6A88"), Offset = 0 },
                    new GradientStop { Color = Color.Parse("#FF99AC"), Offset = 1 }
                }
            }
        };

        _subtitleText = new TextBlock
        {
            Text = "Your GitLab Ops Companion",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 18,
            Opacity = 0,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            Margin = new Thickness(0, 12, 0, 0)
        };

        _contentStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _contentStack.Children.Add(_mainText);
        _contentStack.Children.Add(_subtitleText);

        Content = new Grid
        {
            Background = new SolidColorBrush(Color.Parse("#FAFAFA")),
            Children = { _contentStack }
        };

        _mainText.Transitions = new Transitions
        {
            new DoubleTransition { Property = TextBlock.OpacityProperty, Duration = _fadeDuration }
        };

        _subtitleText.Transitions = new Transitions
        {
            new DoubleTransition { Property = TextBlock.OpacityProperty, Duration = _fadeDuration }
        };

        _ = RunSequenceAsync();
    }

    private async Task RunSequenceAsync()
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() => _mainText.Opacity = 1.0);
            await Task.Delay(200);
            await Dispatcher.UIThread.InvokeAsync(() => _subtitleText.Opacity = 1.0);
            await Task.Delay(1500);
            await Dispatcher.UIThread.InvokeAsync(() => 
            {
                _mainText.Opacity = 0.0;
                _subtitleText.Opacity = 0.0;
            });
            await Task.Delay(_fadeDuration);
        }
        catch
        {
        }

        if (DataContext is ViewModels.WelcomeViewModel vm)
        {
            vm.OnAnimationFinished();
        }
    }
}

