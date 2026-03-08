using System.Text.Json;

namespace FantaniaLib;

public class ScriptObject : FantaniaObject
{
    public ScriptTemplate Template => _script;

    public ScriptObject(ScriptTemplate script)
    {
        _script = script;
        _serializableFields.AddRange(_script.GetDefinedFields());
        _fieldValues = new Dictionary<string, object?>(_script.GetDefinedFields().Count);
        foreach (var fieldInfo in _script.GetDefinedFields())
        {
            _fieldValues.Add(fieldInfo.FieldName, _script.GetDefaultValue(fieldInfo.FieldName));
        }
    }

    public override object? GetFieldValue(string fieldName)
    {
        if (_fieldValues.TryGetValue(fieldName, out object? val))
        {
            return val;
        }
        return base.GetFieldValue(fieldName);
    }

    public override void SetFieldValue(string fieldName, object? value)
    {
        if (_fieldValues.TryGetValue(fieldName, out object? oldVal))
        {
            if (oldVal != value)
            {
                OnPropertyChanging(fieldName);
                _fieldValues[fieldName] = value;
                OnPropertyChanged(fieldName);
            }
            return;
        }
        base.SetFieldValue(fieldName, value);
    }

    public override IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var defFields = _script.GetDefinedFields();
        var editableFields = new List<IEditableField>(defFields.Count);
        foreach (FieldInfo info in defFields)
        {
            if (!_script.CanEditField(info.FieldName))
                continue;
            string fieldName = info.FieldName;
            var editableField = new ScriptObjectEditableField(workspace, this, fieldName);
            editableFields.Add(editableField);
        }
        editableFields.AddRange(base.GetEditableFields(workspace));
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
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

    protected ScriptTemplate _script;
    protected Dictionary<string, object?> _fieldValues;
}