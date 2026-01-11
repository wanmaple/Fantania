using System.Globalization;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public static class TypeConverters
{
    public static IValueConverter TypeIs = new TypeEqualConverter();
}

public class TypeEqualConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return false;
        if (parameter == null) return false;
        var type = value.GetType();
        Type otherType = (Type)parameter;
        return type.IsAssignableTo(otherType);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}