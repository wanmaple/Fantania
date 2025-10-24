using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class OptionValue2NameMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not int || values[1] is not IReadOnlyDictionary<int, string>) return AvaloniaProperty.UnsetValue;
        var map = values[1] as IReadOnlyDictionary<int, string>;
        int val = (int)values[0];
        return map[val];
    }
}
