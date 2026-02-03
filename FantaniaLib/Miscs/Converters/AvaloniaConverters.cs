using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public static class AvaloniaConverters
{
    public static IValueConverter Rectf2AvaRect = new Rectf2RectConverter();
}

public class Rectf2RectConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Rectf rectf) return AvaloniaProperty.UnsetValue;
        return rectf.ToAvaRect();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}