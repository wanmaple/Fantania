using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FantaniaLib;

internal class Vector42ColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Vector4 vec) return AvaloniaProperty.UnsetValue;
        return vec.ToColor();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Color color) return AvaloniaProperty.UnsetValue;
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}

public partial class ColorPicker : UserControl
{
    public ColorPicker()
    {
        InitializeComponent();
    }
}