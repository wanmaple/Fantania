using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class SelectionBox2RectConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 4) return AvaloniaProperty.UnsetValue;
        if (!values.All(o => o is float)) return AvaloniaProperty.UnsetValue;
        double l = System.Convert.ToDouble(values[0]);
        double t = System.Convert.ToDouble(values[1]);
        double r = System.Convert.ToDouble(values[2]);
        double b = System.Convert.ToDouble(values[3]);
        return new Rect(l, t, r - l, b - t);
    }
}