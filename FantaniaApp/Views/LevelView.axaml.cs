using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FantaniaLib;

namespace Fantania.Views;

public class BoundingBox2CanvasRectConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 3) return AvaloniaProperty.UnsetValue;
        if (values[0] is not Rectf bounds || values[1] is not ILevelCanvas canvas) return AvaloniaProperty.UnsetValue;
        Vector2 tlWorld = bounds.TopLeft;
        Vector2 brWorld = bounds.BottomRight;
        Vector2 tlCanvas = canvas.WorldToCanvas(tlWorld);
        Vector2 brCanvas = canvas.WorldToCanvas(brWorld);
        return new Rect(tlCanvas.X, tlCanvas.Y, brCanvas.X - tlCanvas.X, brCanvas.Y - tlCanvas.Y);
    }
}

public class NodeIndex2ColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not int ndIdx) return AvaloniaProperty.UnsetValue;
        if (ndIdx <= 0)
            return Color.Parse("#dede1c");
        return Color.Parse("#9f27a7");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class LevelView : UserControl
{
    public LevelView()
    {
        InitializeComponent();
    }
}