using System.Reflection;
using System.Text.Json;

namespace FantaniaLib;

public abstract class FantaniaObject : SyncableObject, ISerializableData, IEditableObject
{
    public virtual IReadOnlyList<FieldInfo> SerializableFields => _serializableFields;

    protected FantaniaObject()
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

    public abstract string OnCopy(IWorkspace workspace);
    public abstract void OnPaste(IWorkspace workspace, string serializedData);

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

    internal void RaisePropertyChanging(string propName)
    {
        OnPropertyChanging(propName);
    }

    internal void RaisePropertyChanged(string propName)
    {
        OnPropertyChanged(propName);
    }

    protected List<FieldInfo> _serializableFields;
}