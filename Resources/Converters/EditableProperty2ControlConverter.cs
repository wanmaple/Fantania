using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Fantania;

public class EditableProperty2ControlConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        IEditableProperty prop = value as IEditableProperty;
        if (prop == null)
            return AvaloniaProperty.UnsetValue;
        Control ret = null;
        if (ReflectionHelper.IsArrayType(prop.PropertyInfo.PropertyType))
            ret = Activator.CreateInstance<EditArrayControl>();
        else
        {
            if (prop.EditInfo.ControlType != null)
                ret = Activator.CreateInstance(prop.EditInfo.ControlType) as Control;
            else
            {
                // use default mapping.
                if (DEFAULT_TYPE_MAPPING.TryGetValue(prop.EditType, out Type controlType))
                    ret = Activator.CreateInstance(controlType) as Control;
            }
        }
        return ret;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static readonly Dictionary<Type, Type> DEFAULT_TYPE_MAPPING = new Dictionary<Type, Type> {
        { typeof(ConstantAttribute), typeof(ConstantControl) },
        { typeof(ConstantOptionAttribute), typeof(ConstantOptionControl) },
        { typeof(EditBooleanAttribute), typeof(EditBooleanControl) },
        { typeof(EditIntegerAttribute), typeof(EditIntegerControl) },
        { typeof(EditDecimalAttribute), typeof(EditDecimalControl) },
        { typeof(EditStringAttribute), typeof(EditStringControl) },
        { typeof(EditNameAttribute), typeof(EditNameControl) },
        { typeof(EditAngleAttribute), typeof(EditAngleControl) },
        { typeof(EditEnumAttribute), typeof(EditEnumControl) },
        { typeof(EditVector2Attribute), typeof(EditVector2Control) },
        { typeof(EditScaleAttribute), typeof(EditScaleControl) },
        { typeof(EditAnchorAttribute), typeof(EditAnchorControl) },
        { typeof(EditColorAttribute), typeof(ColorPickerControl) },
        { typeof(EditCurveAttribute), typeof(EditCurveControl) },
        { typeof(EditGradient1DAttribute), typeof(EditGradient1DControl) },
        { typeof(EditGradient2DAttribute), typeof(EditGradient2DControl) },
        { typeof(EditNoise2DAttribute), typeof(EditNoise2DControl) },
        { typeof(EditVector4Attribute), typeof(EditVector4Control) },
        { typeof(EditCurvedEdgeAttribute), typeof(EditCurvedEdgeControl) },
        { typeof(EditGroupReferenceAttribute), typeof(EditGroupReferenceControl) },
        { typeof(EditTypeReferenceAttribute), typeof(EditTypeReferenceControl) },
    };
}