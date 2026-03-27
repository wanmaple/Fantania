namespace FantaniaLib;

public class UserGameData : DatabaseObject
{
    public override string TypeName => _script.TypeName;
    public override string GroupName => _script.DataGroup;

    public GameDataTemplate Template => _script;

    public UserGameData(GameDataTemplate template, int id)
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

    public override IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var defFields = _script.GetDefinedFields();
        var editableFields = new List<IEditableField>(defFields.Count);
        foreach (FieldInfo info in defFields)
        {
            if (!_script.CanEditField(info.FieldName))
                continue;
            string fieldName = info.FieldName;
            var editableField = new UserGameDataEditableField(workspace, this, fieldName);
            editableFields.Add(editableField);
        }
        editableFields.AddRange(base.GetEditableFields(workspace));
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
    }

    GameDataTemplate _script;
    Dictionary<string, object?> _fieldValues;
}