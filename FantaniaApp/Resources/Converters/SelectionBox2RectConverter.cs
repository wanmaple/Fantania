using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.Views;

namespace Fantania;

public class SelectionBox2RectConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 5) return AvaloniaProperty.UnsetValue;
        for (int i = 0; i < 4; i++)
        {
            if (values[i] is not float) return AvaloniaProperty.UnsetValue;
        }
        if (values[4] is not ILevelCanvas) return AvaloniaProperty.UnsetValue;
        double l = System.Convert.ToDouble(values[0]);
        double t = System.Convert.ToDouble(values[1]);
        double r = System.Convert.ToDouble(values[2]);
        double b = System.Convert.ToDouble(values[3]);
        ILevelCanvas canvas = (ILevelCanvas)values[4]!;
        try
        {
            Vector2 topLeft = canvas.WorldPositionToCanvasPosition(new Vector2((float)l, (float)t));
            Vector2 bottomRight = canvas.WorldPositionToCanvasPosition(new Vector2((float)r, (float)b));
            return new Rect(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }
        catch
        {
            return AvaloniaProperty.UnsetValue;
        }
    }
}