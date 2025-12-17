using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.Animation;
using Avalonia.Media;

namespace TanukiPanel.Views;

/// <summary>
/// A welcome view with GNOME styling that fades in a message, waits, then fades out and signals completion.
/// </summary>
public class WelcomeView : UserControl
{
    private readonly StackPanel _contentStack;
    private readonly TextBlock _mainText;
    private readonly TextBlock _subtitleText;
    private readonly TextBlock _linkText;
    private readonly Button _backButton;
    private readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(800);

    public WelcomeView()
    {
        // GNOME color scheme
        var gnomeText = Color.Parse("#2E2E2E");
        var gnomeBackground = Color.Parse("#F6F5F4");
        var gnomeBlue = Color.Parse("#3584E4");

        _mainText = new TextBlock
        {
            Text = "Tanuki Panel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 48,
            FontWeight = FontWeight.Bold,
            Opacity = 0,
            Foreground = new SolidColorBrush(gnomeBlue)
        };

        _subtitleText = new TextBlock
        {
            Text = "Your GitLab Ops Companion",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 18,
            Opacity = 0,
            Foreground = new SolidColorBrush(Color.Parse("#77767B")),
            Margin = new Thickness(0, 12, 0, 0)
        };

        _linkText = new TextBlock
        {
            Text = "How to find my API key",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            Opacity = 0,
            Foreground = new SolidColorBrush(gnomeBlue),
            Margin = new Thickness(0, 24, 0, 0),
            TextDecorations = TextDecorations.Underline,
            Cursor = new Cursor(StandardCursorType.Hand)
        };

        _backButton = new Button
        {
            Content = "Continue to API Key",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0),
            Padding = new Thickness(16, 10, 16, 10),
            Background = new SolidColorBrush(gnomeBlue),
            Foreground = new SolidColorBrush(Color.Parse("#FFFFFF")),
            FontSize = 14,
            Opacity = 0
        };

        _contentStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        _contentStack.Children.Add(_mainText);
        _contentStack.Children.Add(_subtitleText);
        _contentStack.Children.Add(_linkText);
        _contentStack.Children.Add(_backButton);

        Content = new Grid
        {
            Background = new SolidColorBrush(gnomeBackground),
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

        _linkText.Transitions = new Transitions
        {
            new DoubleTransition { Property = TextBlock.OpacityProperty, Duration = _fadeDuration }
        };

        _backButton.Transitions = new Transitions
        {
            new DoubleTransition { Property = Button.OpacityProperty, Duration = _fadeDuration }
        };

        // Add click handler to the link
        _linkText.PointerPressed += (s, e) =>
        {
            if (DataContext is ViewModels.WelcomeViewModel vm)
            {
                vm.ShowApiKeyGuideCommand.Execute(null);
            }
        };

        // Add click handler to the back button
        _backButton.Click += (s, e) =>
        {
            if (DataContext is ViewModels.WelcomeViewModel vm)
            {
                vm.OnAnimationFinished();
            }
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
            await Task.Delay(200);
            await Dispatcher.UIThread.InvokeAsync(() => _linkText.Opacity = 1.0);
            await Task.Delay(200);
            await Dispatcher.UIThread.InvokeAsync(() => _backButton.Opacity = 1.0);
        }
        catch
        {
        }
    }
}

