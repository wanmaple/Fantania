using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace FantaniaLib;

public class EditableField2EditControlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return AvaloniaProperty.UnsetValue;
        if (value is not IEditableField field) return AvaloniaProperty.UnsetValue;
        Type controlType = field.EditInfo.EditControlType;
        if (controlType != null)
        {
            UserControl uc = Activator.CreateInstance(controlType) as UserControl;
            uc.DataContext = field;
            return uc;
        }
        else
        {
            // FieldValue must never be null, that means a default value should never be null.
            Type type = field.FieldValue.GetType();
            if (DEFAULT_CONTROL_MAP.TryGetValue(type, out Type ctrlType))
            {
                UserControl uc = Activator.CreateInstance(ctrlType) as UserControl;
                uc.DataContext = field;
                return uc;
            }
        }
        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    static readonly IReadOnlyDictionary<Type, Type> DEFAULT_CONTROL_MAP = new Dictionary<Type, Type>
    {
        { typeof(int), typeof(IntegerBox) },
        { typeof(bool), typeof(CheckBox) },
        { typeof(float), typeof(FloatBox) },
        { typeof(string), typeof(StringBox) },
        { typeof(Vector2), typeof(Vector2Box) },
        { typeof(Vector4), typeof(ColorPicker) },
    };
}

public class TooltipConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not string content || values[1] is not IValueConverter converter) return AvaloniaProperty.UnsetValue;
        if (converter != null)
            return converter.Convert(content, typeof(string), null, culture);
        return content;
    }
}