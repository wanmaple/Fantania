using System.Reflection;

namespace FantaniaLib;

public abstract class DatabaseObject : SyncableObject, ISerializableData, IEditableObject
{
    public virtual IReadOnlyList<FieldInfo> SerializableFields => _serializableFields;

    /// <summary>
    /// Refers to the name of table in database.
    /// </summary>
    public abstract string TypeName { get; }
    /// <summary>
    /// Objects with different types but the same group will be ID-distinct.
    /// </summary>
    public abstract string GroupName { get; }

    public int ID { get; protected set; }

    private string _name = string.Empty;
    [SerializableField(FieldTypes.String), EditableField(TooltipKey = "TT_Name")]
    public string Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {
                OnPropertyChanging(nameof(Name));
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    private string _tooltip = string.Empty;
    [SerializableField(FieldTypes.String), EditableField(TooltipKey = "TT_Tooltip")]
    public string Tooltip
    {
        get { return _tooltip; }
        set
        {
            if (_tooltip != value)
            {
                OnPropertyChanging(nameof(Tooltip));
                _tooltip = value;
                OnPropertyChanged(nameof(Tooltip));
            }
        }
    }

    protected DatabaseObject(int id)
    {
        ID = id;
        
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

    protected List<FieldInfo> _serializableFields;
}