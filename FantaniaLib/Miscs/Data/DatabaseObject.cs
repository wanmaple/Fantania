using System.Reflection;
using System.Text.Json;

namespace FantaniaLib;

public abstract class DatabaseObject : SyncableObject, ISerializableData, IEditableObject
{
    public virtual IReadOnlyList<FieldInfo> SerializableFields => _serializableFields;

    /// <summary>
    /// 自身所属类型，也代表数据库中的表名。
    /// </summary>
    public abstract string TypeName { get; }
    /// <summary>
    /// 不同类型但是相同分组的对象必须是各自ID唯一的，并且存在分组的对象会在数据库中构建一张xxxGroup的表用于检索自身所属类型。
    /// </summary>
    public abstract string GroupName { get; }

    public int ID { get; protected set; }

    private string _name = string.Empty;
    [SerializableField(FieldTypes.String), EditableField(TooltipKey = "TT_Name")]
    public virtual string Name
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

    public virtual string GetDisplayName(IWorkspace workspace)
    {
        return TypeName;
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
        var obj = JsonSerializer.Deserialize<Dictionary<string, object?>>(serializedData);
        if (obj == null) return;
        var data = obj["Data"] as Dictionary<string, string>;
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