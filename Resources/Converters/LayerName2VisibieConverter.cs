using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class LayerName2VisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string layerName = value as string;
        RenderLayers layer = Enum.Parse<RenderLayers>(layerName);
        return WorkspaceViewModel.Current.Workspace.CurrentWorld.IsLayerVisible(layer);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}