
using System.Reflection;
using System.Text.Json;

namespace FantaniaLib;

public abstract class BinaryObject : FantaniaObject
{
    public virtual string ClassName => GetType().FullName!;

    protected BinaryObject()
    {
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
}