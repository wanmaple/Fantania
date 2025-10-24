using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Fantania;

public class Pivot2RectFillConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not GradientPivot || values[1] is not int) return AvaloniaProperty.UnsetValue;
        GradientPivot pivot = values[0] as GradientPivot;
        int selectedIdx = (int)values[1];
        if (pivot.Index == selectedIdx)
            return new SolidColorBrush(pivot.Color.ToColor(), pivot.Color.W);
        return Brushes.Transparent;
    }
}
