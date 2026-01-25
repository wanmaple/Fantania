
using System.Reflection;

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

    public IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
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

    protected List<FieldInfo> _serializableFields;
}