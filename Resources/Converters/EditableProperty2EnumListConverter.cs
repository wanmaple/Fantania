using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;

namespace Fantania;

public class EditableProperty2EnumListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        IEditableProperty prop = value as IEditableProperty;
        EditEnumAttribute enumAttr = prop.EditInfo as EditEnumAttribute;
        return Enum.GetNames(prop.EditValue.GetType()).Except(enumAttr.Excepts);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
