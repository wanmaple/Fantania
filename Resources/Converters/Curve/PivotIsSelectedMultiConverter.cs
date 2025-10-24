using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class PivotIsSelectedMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not int || values[1] is not int) return AvaloniaProperty.UnsetValue;
        int index = (int)values[0];
        int selectedIdx = (int)values[1];
        return index == selectedIdx;
    }
}
