using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Fantania.Models;

namespace Fantania;

public class Placement2EnabledConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return false;
        return value is StylegroundTemplate;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
