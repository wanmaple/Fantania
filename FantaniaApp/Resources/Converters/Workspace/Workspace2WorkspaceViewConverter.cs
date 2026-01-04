using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Fantania.ViewModels;
using Fantania.Views;
using FantaniaLib;

namespace Fantania;

public class Workspace2WorkspaceViewConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not Workspace workspace) return AvaloniaProperty.UnsetValue;
        var v = new WorkspaceView();
        var vm = new WorkspaceViewModel(workspace);
        v.DataContext = vm;
        return v;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}