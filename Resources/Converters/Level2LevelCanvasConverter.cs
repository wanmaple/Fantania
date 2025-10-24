using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.Models;

namespace Fantania;

public class Level2LevelCanvasConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        Level lv = value as Level;
        return new LevelCanvas(lv);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}