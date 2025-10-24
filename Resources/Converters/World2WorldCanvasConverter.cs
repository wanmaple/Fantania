using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.Models;

namespace Fantania;

public class World2WorldCanvasConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        World world = value as World;
        return new WorldCanvas(world);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}