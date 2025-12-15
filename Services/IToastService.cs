using System;

namespace TanukiPanel.Services;

/// <summary>
/// Service for displaying toast notifications
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Shows a toast notification with the given message
    /// </summary>
    void ShowToast(string message, ToastType type = ToastType.Info, int durationMs = 3000);
}

public enum ToastType
{
    Success,
    Error,
    Info,
    Warning
}

public class ToastService : IToastService
{
    public event EventHandler<ToastEventArgs>? ToastRequested;

    public void ShowToast(string message, ToastType type = ToastType.Info, int durationMs = 3000)
    {
        ToastRequested?.Invoke(this, new ToastEventArgs { Message = message, Type = type, DurationMs = durationMs });
    }
}

public class ToastEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; }
    public int DurationMs { get; set; }
}
