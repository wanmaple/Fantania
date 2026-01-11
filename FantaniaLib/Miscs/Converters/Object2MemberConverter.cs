using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class Object2MemberConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (parameter == null) return AvaloniaProperty.UnsetValue;
        if (parameter is not string args) return AvaloniaProperty.UnsetValue;
        string[] memberPath = args.Split('.', StringSplitOptions.RemoveEmptyEntries);
        object? current = value;
        foreach (string memberName in memberPath)
        {
            current = current.GetValueOfFieldOrProperty(memberName);
            if (current == null)
                return AvaloniaProperty.UnsetValue;
        }
        return current;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}