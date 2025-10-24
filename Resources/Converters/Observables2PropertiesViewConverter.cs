using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using Fantania.ViewModels;
using Fantania.Views;

namespace Fantania;

public class Observables2PropertiesViewConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        IEnumerable array = value as IEnumerable;
        var instances = new List<ObservableObject>();
        foreach (ObservableObject obj in array)
        {
            instances.Add(obj);
        }
        if (instances.Count == 0) return null;
        var ret = new PropertiesView();
        if (instances.Count == 1)
        {
            ret.DataContext = new PropertiesViewModel(ObjectEditableCollector.CollectEditableProperties(instances[0]));
        }
        else
        {
            ret.DataContext = new PropertiesViewModel(ObjectEditableCollector.CollectShareableEditableProperties(instances));
        }
        return ret;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}