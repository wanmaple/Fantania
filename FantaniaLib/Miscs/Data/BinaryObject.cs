
using System.Reflection;
using System.Text.Json;

namespace FantaniaLib;

public abstract class BinaryObject : SyncableObject, ISerializableData, IEditableObject
{
    public virtual string ClassName => GetType().FullName!;
    public virtual IReadOnlyList<FieldInfo> SerializableFields => _serializableFields;

    protected BinaryObject()
    {
        var props = GetPropertiesWithAttribute<SerializableFieldAttribute>();
        _serializableFields = new List<FieldInfo>(props.Count);
        foreach (PropertyInfo prop in props)
        {
            SerializableFieldAttribute attr = prop.GetCustomAttribute<SerializableFieldAttribute>()!;
            _serializableFields.Add(new FieldInfo
            {
                FieldName = prop.Name,
                FieldType = attr.FieldType,
            });
        }
    }

    public virtual IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var editableFields = new List<IEditableField>();
        var props = GetPropertiesWithAttribute<EditableFieldAttribute>();
        foreach (PropertyInfo prop in props)
        {
            var editableField = new SingleObjectEditableField(workspace, this, prop);
            editableFields.Add(editableField);
        }
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
    }

    public virtual object? GetFieldValue(string fieldName)
    {
        var prop = GetType().GetProperty(fieldName);
        if (prop != null)
        {
            return prop.GetValue(this);
        }
        return null;
    }

    public virtual void SetFieldValue(string fieldName, object? value)
    {
        var prop = GetType().GetProperty(fieldName);
        if (prop != null)
        {
            prop.SetValue(this, value);
        }
    }

    public virtual string OnCopy(IWorkspace workspace)
    {
        var rule = SerializationRule.Default;
        var obj = new Dictionary<string, object?>();
        obj["Type"] = GetType().ToString();
        var data = new Dictionary<string, string>();
        foreach (var field in SerializableFields)
        {
            var value = GetFieldValue(field.FieldName);
            var serializedValue = rule.CastTo(field.FieldType, value, this);
            data[field.FieldName] = serializedValue;
        }
        obj["Data"] = data;
        JsonSerializerOptions option = new JsonSerializerOptions
        {
            WriteIndented = false,
            IncludeFields = true,
        };
        return JsonSerializer.Serialize(obj, option);
    }

    public virtual void OnPaste(IWorkspace workspace, string serializedData)
    {
        var rule = SerializationRule.Default;
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedData);
        if (data == null) return;
        foreach (var field in SerializableFields)
        {
            if (data.TryGetValue(field.FieldName, out var serializedValue))
            {
                var value = rule.CastFrom(field.FieldType, serializedValue, this);
                SetFieldValue(field.FieldName, value);
            }
        }
    }

    protected List<FieldInfo> _serializableFields;
}