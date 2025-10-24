using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Fantania;

public class Pivots2GradientStopsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        IList<GradientPivot> pivots = value as IList<GradientPivot>;
        GradientStops stops = new GradientStops();
        foreach (var pivot in pivots)
        {
            var stop = new GradientStop(pivot.Color.ToColor(), (double)pivot.Index / (Gradient1D.SEGMENTS - 1));
            stops.Add(stop);
        }
        return stops;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
