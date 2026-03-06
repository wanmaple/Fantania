using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FantaniaLib;

public class JsonDataSyncer<T> where T : ISerializableData
{
    class JsonObjectData
    {
        public string ObjectType { get; set; } = string.Empty;
        public Dictionary<string, object> ObjectData { get; set; } = new Dictionary<string, object>();
    }

    public JsonDataSyncer(T data, SerializationRule rule)
    {
        _data = data;
        _rule = rule;
    }

    public void SyncFromJson(string json)
    {
        JsonObjectData data = JsonSerializer.Deserialize<JsonObjectData>(json)!;
        DeserializeObject(data, _data);
    }

    public string SyncToJson()
    {
        var dict = SerializeObject(_data);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(dict, options);
    }

    JsonObjectData SerializeObject(ISerializableData data)
    {
        var ret = new JsonObjectData();
        ret.ObjectType = data.GetType().FullName!;
        var fields = data.SerializableFields;
        foreach (var field in fields)
        {
            object value = data.GetFieldValue(field.FieldName)!;
            string str = _rule.CastTo(field.FieldType, value, data);
            ret.ObjectData.Add(field.FieldName, str);
        }
        foreach (var prop in data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (typeof(ISerializableData).IsAssignableFrom(prop.PropertyType))
            {
                var subData = (ISerializableData)prop.GetValue(data)!;
                var data2 = SerializeObject(subData);
                ret.ObjectData.Add(prop.Name, data2);
            }
        }
        return ret;
    }

    void DeserializeObject(JsonObjectData jsonData, ISerializableData data)
    {
        var fields = data.SerializableFields;
        foreach (var pair in jsonData.ObjectData)
        {
            string fieldName = pair.Key;
            object value = pair.Value!;
            var field = fields.FirstOrDefault(f => f.FieldName == fieldName);
            if (field != null)
            {
                object? val = _rule.CastFrom(field.FieldType, value.ToString()!, data);
                data.SetFieldValue(fieldName, val);
            }
            else
            {
                var prop = data.GetType().GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && typeof(ISerializableData).IsAssignableFrom(prop.PropertyType))
                {
                    ISerializableData subdata = (ISerializableData)prop.GetValue(data)!;
                    JsonObjectData jsonObj = JsonSerializer.Deserialize<JsonObjectData>((JsonElement)value)!;
                    DeserializeObject(jsonObj, subdata);
                }
            }
        }
    }

    T _data;
    SerializationRule _rule;
}