using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class Value2PositionYMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not float || values[1] is not double) return AvaloniaProperty.UnsetValue;
        float value = 1.0f - (float)values[0];
        double height = (double)values[1] - EditCurveControl.PADDING_2;
        double ret = value * height + EditCurveControl.PADDING;
        return ret;
    }
}
