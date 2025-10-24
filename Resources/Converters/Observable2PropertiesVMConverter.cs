using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using Fantania.ViewModels;

namespace Fantania;

public class Observable2PropertiesVMConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        ObservableObject obj = value as ObservableObject;
        return new PropertiesViewModel(ObjectEditableCollector.CollectEditableProperties(obj));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}