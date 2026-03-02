using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class EnumType2OptionsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Type type) return AvaloniaProperty.UnsetValue;
        if (!type.IsEnum) return AvaloniaProperty.UnsetValue;
        var values = Enum.GetValues(type);
        var options = new List<Option>(values.Length);
        string[]? excepts = null;
        if (parameter != null && parameter is string exceptStr)
        {
            excepts = exceptStr.Split(',');
        }
        foreach (object val in values)
        {
            string name = Enum.GetName(type, val)!;
            if (excepts != null && excepts.Any(s => s == name)) continue;
            options.Add(new Option
            {
                Name = name,
                Value = val,
            });
        }
        return options;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumObject2OptionsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        Type type = value.GetType();
        if (!type.IsEnum) return AvaloniaProperty.UnsetValue;
        var values = Enum.GetValues(type);
        var options = new List<Option>(values.Length);
        string[]? excepts = null;
        if (parameter != null && parameter is string exceptStr)
        {
            excepts = exceptStr.Split(',');
        }
        foreach (object val in values)
        {
            string name = Enum.GetName(type, val)!;
            if (excepts != null && excepts.Any(s => s == name)) continue;
            options.Add(new Option
            {
                Name = name,
                Value = val,
            });
        }
        return options;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}