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
        Type type = field.FieldValue.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FantaniaArray<>))
        {
            UserControl uc = new ArrayBox();
            uc.DataContext = field;
            return uc;
        }
        Type? controlType = field.EditInfo.EditControlType;
        if (controlType != null)
        {
            UserControl? uc = Activator.CreateInstance(controlType) as UserControl;
            if (uc == null) return AvaloniaProperty.UnsetValue;
            uc.DataContext = field;
            return uc;
        }
        else
        {
            if (DEFAULT_CONTROL_MAP.TryGetValue(type, out Type? ctrlType))
            {
                UserControl? uc = Activator.CreateInstance(ctrlType) as UserControl;
                if (uc == null) return AvaloniaProperty.UnsetValue;
                uc.DataContext = field;
                return uc;
            }
            else if (type.IsEnum)
            {
                EnumBox uc = new EnumBox();
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
        { typeof(Vector2Int), typeof(Vector2IntBox) },
        { typeof(Vector3), typeof(Vector3Box) },
        { typeof(Vector4), typeof(ColorPicker) },
        { typeof(Direction3D), typeof(Direction3DBox) },
        { typeof(TypeReference), typeof(TypeReferenceBox) },
        { typeof(GroupReference), typeof(GroupReferenceBox) },
        { typeof(TextureDefinition), typeof(TextureBox) },
    };
}

public class Object2GroupedEditFieldsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count != 2) return AvaloniaProperty.UnsetValue;
        if (values[0] is not IEditableObject editable) return AvaloniaProperty.UnsetValue;
        if (values[1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        var fields = editable.GetEditableFields(workspace);
        if (parameter != null && parameter is string excepts)
        {
            HashSet<string> exceptSet = new HashSet<string>(excepts.Split(',').Select(s => s.Trim()));
            fields = fields.Where(f => !exceptSet.Contains(f.FieldName)).ToList();
        }
        return fields.GroupBy(f => f.EditInfo.EditGroup).OrderBy(g => g.Key).Select(g => new GroupedEditableFields
        {
            Workspace = workspace,
            Group = g.Key,
            Fields = new EditableFields(workspace, g),
        });
    }
}

public class Objects2GroupedEditFieldsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null) return AvaloniaProperty.UnsetValue;
        if (values.Count < 2) return AvaloniaProperty.UnsetValue;
        for (int i = 0; i < values.Count - 1; i++)
        {
            if (values[i] is not IEditableObject) return AvaloniaProperty.UnsetValue;
        }
        if (values[values.Count - 1] is not IWorkspace workspace) return AvaloniaProperty.UnsetValue;
        List<IEditableField> fields = new List<IEditableField>();
        for (int i = 0; i < values.Count - 1; i++)
        {
            IEditableObject editable = (IEditableObject)values[i]!;
            fields.AddRange(editable.GetEditableFields(workspace));
        }
        if (parameter != null && parameter is string excepts)
        {
            HashSet<string> exceptSet = new HashSet<string>(excepts.Split(',').Select(s => s.Trim()));
            fields = fields.Where(f => !exceptSet.Contains(f.FieldName)).ToList();
        }
        return fields.GroupBy(f => f.EditInfo.EditGroup).OrderBy(g => g.Key).Select(g => new GroupedEditableFields
        {
            Workspace = workspace,
            Group = g.Key,
            Fields = new EditableFields(workspace, g),
        });
    }
}