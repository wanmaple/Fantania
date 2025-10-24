using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.ViewModels;

namespace Fantania;

public class LevelName2VisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string name = value as string;
        if (name != WorkspaceViewModel.Current.Workspace.CurrentLevel.Name)
            return true;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LevelName2InvertVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        string name = value as string;
        if (name != WorkspaceViewModel.Current.Workspace.CurrentLevel.Name)
            return false;
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
