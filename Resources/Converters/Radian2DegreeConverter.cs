using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class Radian2DegreeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double rad = (double)value;
        return double.RadiansToDegrees(rad);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double deg = System.Convert.ToDouble(value);
        return double.DegreesToRadians(deg);
    }
}