using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TanukiPanel.Views.Converters;

public class ProgressPercentageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is double progress)
        {
            return $"Progress: {(int)progress}%";
        }
        return "Progress: 0%";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

