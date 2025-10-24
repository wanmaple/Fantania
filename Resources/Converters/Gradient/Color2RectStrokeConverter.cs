using System;
using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Fantania;

public class Color2RectStrokeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        Vector4 color = (Vector4)value;
        float grayscale = Vector3.Dot(new Vector3(color.X, color.Y, color.Z), new Vector3(0.299f, 0.587f, 0.114f));
        return grayscale <= 0.5 ? Brushes.White : Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
