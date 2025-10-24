using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class EnumInfo
{
    public Type EnumType { get; set; }
}

public class EnumValue2StringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        (parameter as EnumInfo).EnumType = value.GetType();
        return value.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;
        string enumName = value as string;
        return Enum.Parse((parameter as EnumInfo).EnumType, enumName);
    }
}
