using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public static class NumberConverters
{
    public static IValueConverter Float2Decimal = new Float2DecimalConverter();
    public static IValueConverter Integer2Decimal = new Integer2DecimalConverter();
    public static IValueConverter Integer2String = new Integer2StringConverter();
    public static IValueConverter IntegerEqual = new IntegerEqualConverter();
    public static IValueConverter IntegerNotEqual = new IntegerNotEqualConverter();
    public static IValueConverter IntegerAdd = new IntegerAddConverter();
    public static IValueConverter IntegerMultiple = new IntegerMultipleConverter();
    public static IValueConverter Decimal2String = new Decimal2StringConverter();
    public static IValueConverter DecimalAdd = new DecimalAddConverter();
    public static IValueConverter DecimalMultiple = new DecimalMultipleConverter();
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

public class Integer2StringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        int num1 = System.Convert.ToInt32(value);
        return num1.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string str = value.ToString();
        if (int.TryParse(str, out int num))
        {
            return num;
        }
        return 0;
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

public class IntegerAddConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        int num1 = System.Convert.ToInt32(value);
        int num2 = System.Convert.ToInt32(parameter);
        return num1 + num2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IntegerMultipleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        int num1 = System.Convert.ToInt32(value);
        int num2 = System.Convert.ToInt32(parameter);
        return num1 * num2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DecimalAddConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double num1 = System.Convert.ToDouble(value);
        double num2 = System.Convert.ToDouble(parameter);
        return num1 + num2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DecimalMultipleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double num1 = System.Convert.ToDouble(value);
        double num2 = System.Convert.ToDouble(parameter);
        return num1 * num2;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class Decimal2StringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        double num = System.Convert.ToDouble(value);
        return num.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string str = value as string;
        return double.Parse(str);
    }
}