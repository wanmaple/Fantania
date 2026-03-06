using System.Text.Json;

namespace FantaniaLib;

public abstract class DatabaseObject : FantaniaObject
{
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
    }

    public override string OnCopy(IWorkspace workspace)
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

    public override void OnPaste(IWorkspace workspace, string serializedData)
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

    public virtual string GetDisplayName(IWorkspace workspace)
    {
        return TypeName;
    }
}