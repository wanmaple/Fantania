using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class Tangent2RotationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        float tangent = (float)value;
        float angle = MathF.Atan(tangent);
        return -double.RadiansToDegrees(angle);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}