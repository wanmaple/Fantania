using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Fantania;

public static class TypeConverters
{
    public static IValueConverter TypeIs = new TypeEqualConverter();
}

public class TypeEqualConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return false;
        var type = value.GetType();
        Type otherType = parameter as Type;
        return type.IsAssignableTo(otherType);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}