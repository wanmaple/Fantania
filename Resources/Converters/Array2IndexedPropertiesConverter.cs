using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class Array2IndexedPropertiesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        IEditableProperty prop = value as IEditableProperty;
        object obj = prop.EditValue;
        IEnumerable ary = obj as IEnumerable;
        int idx = 0;
        var ret = new List<IEditableProperty>(16);
        foreach (object _ in ary)
        {
            var indexProp = new IndexedProperty(obj, idx, prop.PropertyInfo, prop.EditInfo);
            ret.Add(indexProp);
            idx++;
        }
        return ret;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}