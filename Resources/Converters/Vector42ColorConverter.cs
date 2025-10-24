using System;
using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Fantania;

public class Vector42ColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        Vector4 vec = (Vector4)value;
        return vec.Linear2Srgb().ToColor();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        Color color = (Color)value;
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f).Srgb2Linear();
    }
}