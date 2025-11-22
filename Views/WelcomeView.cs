using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace TanukiPanel.Views;

/// <summary>
/// A small welcome view that fades in a message, waits, then fades out and signals completion.
/// This uses simple manual opacity animation so no extra packages are required.
/// </summary>
public class WelcomeView : UserControl
{
    private readonly TextBlock _text;
    public event Action? Finished;

    public WelcomeView()
    {
        _text = new TextBlock
        {
            Text = "Welcome to Tanuki Panel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 28,
            Opacity = 0
        };

        Content = new Grid
        {
            Children = { _text }
        };

        _ = RunSequenceAsync();
    }

    private async Task RunSequenceAsync()
    {
        try
        {
            await FadeToAsync(1.0, 700); // fade in 700ms
            await Task.Delay(1200);      // visible for 1.2s
            await FadeToAsync(0.0, 900); // fade out 900ms
        }
        catch
        {
        }

        Finished?.Invoke();
    }

    private async Task FadeToAsync(double target, int durationMs)
    {
        const int stepMs = 16; 
        var steps = Math.Max(1, durationMs / stepMs);
        var start = await Dispatcher.UIThread.InvokeAsync(() => _text.Opacity);
        for (int i = 1; i <= steps; i++)
        {
            var t = (double)i / steps;
            var value = start + (target - start) * t;
            await Dispatcher.UIThread.InvokeAsync(() => _text.Opacity = value);
            await Task.Delay(stepMs);
        }
        await Dispatcher.UIThread.InvokeAsync(() => _text.Opacity = target);
    }
}
