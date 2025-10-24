using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using Fantania.ViewModels;
using Fantania.Views;

namespace Fantania;

public class Observable2PropertiesViewConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        ObservableObject obj = value as ObservableObject;
        if (obj == null) return AvaloniaProperty.UnsetValue;
        var ret = new PropertiesView();
        ret.DataContext = new PropertiesViewModel(ObjectEditableCollector.CollectEditableProperties(obj));
        return ret;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}