using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.ViewModels;

namespace Fantania;

public class WorldName2VisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string worldName = value as string;
        if (worldName != WorkspaceViewModel.Current.Workspace.CurrentWorld.Name)
            return true;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class WorldName2InvertVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string worldName = value as string;
        if (worldName != WorkspaceViewModel.Current.Workspace.CurrentWorld.Name)
            return false;
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
