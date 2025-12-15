using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TanukiPanel.Services;

namespace TanukiPanel.Views;

/// <summary>
/// A toast notification container that displays temporary notifications
/// </summary>
public class ToastContainer : UserControl
{
    private readonly StackPanel _toastStack;
    private readonly Queue<ToastItem> _toastQueue;
    private bool _isDisplaying;

    public ToastContainer(IToastService toastService)
    {
        _toastStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(16)
        };

        _toastQueue = new Queue<ToastItem>();

        Content = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Padding = new Thickness(0),
            Child = _toastStack
        };

        // Subscribe to toast requests
        if (toastService is ToastService ts)
        {
            ts.ToastRequested += (s, e) =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _toastQueue.Enqueue(new ToastItem
                    {
                        Message = e.Message,
                        Type = e.Type,
                        DurationMs = e.DurationMs
                    });

                    if (!_isDisplaying)
                    {
                        _ = DisplayNextToastAsync();
                    }
                });
            };
        }
    }

    private async Task DisplayNextToastAsync()
    {
        if (_toastQueue.Count == 0)
        {
            _isDisplaying = false;
            return;
        }

        _isDisplaying = true;
        var toast = _toastQueue.Dequeue();

        var toastControl = CreateToastControl(toast);
        _toastStack.Children.Add(toastControl);

        await Task.Delay(toast.DurationMs);

        _toastStack.Children.Remove(toastControl);

        await DisplayNextToastAsync();
    }

    private Control CreateToastControl(ToastItem toast)
    {
        var (bgColor, textColor, icon) = toast.Type switch
        {
            ToastType.Success => ("#D4EDDA", "#155724", "✓"),
            ToastType.Error => ("#F8D7DA", "#721C24", "✕"),
            ToastType.Warning => ("#FFF3CD", "#856404", "⚠"),
            _ => ("#D1ECF1", "#0C5460", "ℹ")
        };

        var iconBlock = new TextBlock
        {
            Text = icon,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse(textColor)),
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 2, 8, 0)
        };

        var messageBlock = new TextBlock
        {
            Text = toast.Message,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse(textColor)),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 300
        };

        var contentStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 0
        };

        contentStack.Children.Add(iconBlock);
        contentStack.Children.Add(messageBlock);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse(bgColor)),
            BorderBrush = new SolidColorBrush(Color.Parse(textColor)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = contentStack
        };

        return border;
    }

    private class ToastItem
    {
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; }
        public int DurationMs { get; set; }
    }
}
