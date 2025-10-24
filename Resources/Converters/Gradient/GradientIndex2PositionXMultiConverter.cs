using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class GradientIndex2PositionXMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not int || values[1] is not double) return AvaloniaProperty.UnsetValue;
        int index = (int)values[0];
        double width = (double)values[1];
        double ret = (double)index / (Gradient1D.SEGMENTS - 1) * width + 10.0;
        return ret;
    }
}
