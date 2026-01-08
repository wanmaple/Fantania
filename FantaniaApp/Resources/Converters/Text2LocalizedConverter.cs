using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.Localization;

namespace Fantania;

public class Text2LocalizedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not string text) return AvaloniaProperty.UnsetValue;
        return LocalizationHelper.GetLocalizedString(text);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}