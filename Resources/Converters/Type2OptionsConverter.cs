using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.Models;
using Fantania.ViewModels;

namespace Fantania;

public class Type2OptionsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        Type type = value as Type;
        Workspace workspace = WorkspaceViewModel.Current.Workspace;
        return workspace.MainDatabase.ObjectsOfType(type.Name);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}