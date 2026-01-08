using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using FantaniaLib;

namespace Fantania;

public class GroupedEditableFields
{
    public string Group { get; set; }
    public EditableFields Fields { get; set; }
}

public class Object2GroupedEditFieldsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not IEditableObject editable) return AvaloniaProperty.UnsetValue;
        var fields = editable.EditableFields;
        return fields.GroupBy(f => f.EditInfo.EditGroup).OrderBy(g => g.Key).Select(g => new GroupedEditableFields
        {
            Group = g.Key,
            Fields = new EditableFields(g),
        });
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}