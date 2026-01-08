using System.Diagnostics.CodeAnalysis;

namespace FantaniaLib;

public class UserPlacement : DatabaseObject, IPlacement
{
    public string ClassName => null;
    public string Group => null;

    public IList<IPlacement> Children => null;

    public override string TypeName => _script.ClassName;
    public override string GroupName => string.Empty;
    
    public override IReadOnlyList<IEditableField> EditableFields
    {
        get
        {
            var defFields = _script.GetDefinedFields();
            var editableFields = new List<IEditableField>(defFields.Count);
            foreach (FieldInfo info in defFields)
            {
                string fieldName = info.FieldName;
                var editableField = new UserPlacementEditableField(this, fieldName);
                editableFields.Add(editableField);
            }
            editableFields.AddRange(base.EditableFields);
            editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
            return editableFields;
        }
    }

    internal ScriptTemplate Template => _script;

    public UserPlacement([DisallowNull] ScriptTemplate template, int id)
    : base(id)
    {
        _script = template;
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

    ScriptTemplate _script;
    Dictionary<string, object?> _fieldValues;
}