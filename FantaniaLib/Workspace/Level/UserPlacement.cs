namespace FantaniaLib;

public class UserPlacement : DatabaseObject, IPlacement
{
    public string ClassName => string.Empty;
    public string Group => string.Empty;

    public IReadOnlyList<IPlacement> Children => Array.Empty<IPlacement>();
    public IList<IPlacement> Source => Array.Empty<IPlacement>();

    public override string TypeName => _script.ClassName.MakeFirstCharacterUpper();
    public override string GroupName => string.Empty;

    internal PlacementTemplate Template => _script;
    internal bool FieldDirty => _fieldDirty;

    public UserPlacement(PlacementTemplate template, int id)
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
                _fieldDirty = true;
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
            string fieldName = info.FieldName;
            var editableField = new UserPlacementEditableField(workspace, this, fieldName);
            editableFields.Add(editableField);
        }
        editableFields.AddRange(base.GetEditableFields(workspace));
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
    }

    public IReadOnlyList<LocalRenderInfo> GetRenderInfo(IReadOnlyList<Vector2Int> nodes)
    {
        var ret = Template.GetRenderInfo(this, nodes);
        _fieldDirty = false;
        return ret;
    }

    PlacementTemplate _script;
    Dictionary<string, object?> _fieldValues;
    bool _fieldDirty = true;
}