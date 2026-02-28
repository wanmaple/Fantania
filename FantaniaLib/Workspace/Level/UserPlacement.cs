namespace FantaniaLib;

public class UserPlacement : DatabaseObject, IPlacement
{
    public event Action? FieldChanged;

    public string ClassName => string.Empty;
    public string Group => string.Empty;

    public IReadOnlyList<IPlacement> Children => Array.Empty<IPlacement>();
    public IList<IPlacement> Source => Array.Empty<IPlacement>();

    public override string TypeName => _script.ClassName.MakeFirstCharacterUpper();
    public override string GroupName => string.Empty;

    public PlacementTemplate Template => _script;

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
                FieldChanged?.Invoke();
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
            var editableField = new UserPlacementEditableField(workspace, this, fieldName);
            editableFields.Add(editableField);
        }
        editableFields.AddRange(base.GetEditableFields(workspace));
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
    }

    public override string GetDisplayName(IWorkspace workspace)
    {
        return workspace.LocalizeString(_script.Name);
    }

    public IReadOnlyList<LocalRenderInfo> GetLocalNodeAt(int index, int nodeCnt)
    {
        return Template.GetLocalNodeAt(this, index, nodeCnt);
    }

    public IReadOnlyList<LocalRenderInfo> GetBackgroundNodes(IReadOnlyList<LevelEntityNode> nodes)
    {
        return Template.GetBackgroundNodes(this, nodes);
    }

    public IReadOnlyList<LocalRenderInfo> GetForegroundNodes(IReadOnlyList<LevelEntityNode> nodes)
    {
        return Template.GetForegroundNodes(this, nodes);
    }

    PlacementTemplate _script;
    Dictionary<string, object?> _fieldValues;
}