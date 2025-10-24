using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class EditableProperty2ConstantOptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        IEditableProperty prop = value as IEditableProperty;
        int index = (int)prop.EditValue;
        return (prop.EditInfo as ConstantOptionAttribute).DisplayMap[index];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}