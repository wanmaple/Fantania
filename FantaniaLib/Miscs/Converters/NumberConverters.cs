using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public static class NumberConverters
{
    public static IValueConverter Float2Decimal = new Float2DecimalConverter();
    public static IValueConverter Integer2Decimal = new Integer2DecimalConverter();
    public static IValueConverter IntegerEqual = new IntegerEqualConverter();
    public static IValueConverter IntegerNotEqual = new IntegerNotEqualConverter();
}

public class Float2DecimalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        float val = System.Convert.ToSingle(value);
        return System.Convert.ToDecimal(val);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        decimal val = System.Convert.ToDecimal(value);
        return System.Convert.ToSingle(val);
    }
}

public class Integer2DecimalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        float val = System.Convert.ToInt32(value);
        return System.Convert.ToDecimal(val);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        decimal val = System.Convert.ToDecimal(value);
        return System.Convert.ToInt32(val);
    }
}

public class IntegerEqualConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        int num1 = System.Convert.ToInt32(value);
        int num2 = System.Convert.ToInt32(parameter);
        return num1 == num2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IntegerNotEqualConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        int num1 = System.Convert.ToInt32(value);
        int num2 = System.Convert.ToInt32(parameter);
        return num1 != num2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}