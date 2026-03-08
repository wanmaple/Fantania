using MoonSharp.Interpreter;

namespace FantaniaLib;

public class ScriptTemplate
{
    public class FieldExtraData
    {
        public DynValue DefaultValue { get; set; } = DynValue.Nil;
        public bool CanEdit { get; set; } = false;
        public string EditGroup { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public Type? EditControlType { get; set; } = null;
        public string EditParameter { get; set; } = string.Empty;
        public Type? EditValidatorType { get; set; } = null;
        public object? DefaultMemberValue { get; set; } = null;   // Only used for array-member field defaults, ignored otherwise.
    }

    public string ClassName
    {
        get
        {
            return _engine.GetInstanceMember(_obj, "__clsname").String;
        }
    }

    public string Group => GetOrCallMember("group").String;
    public string Name => GetOrCallMember("name").String;
    public string Tooltip => GetOrCallMember("tooltip").String;

    public ScriptTemplate(ScriptEngine engine, DynValue obj)
    {
        _engine = engine;
        _obj = obj;
    }

    public IReadOnlyList<FieldInfo> GetDefinedFields()
    {
        if (_fieldDefs == null)
        {
            DynValue val = GetOrCallMember("dataDefs");
            if (val.Type != DataType.Table)
            {
                _fieldDefs = Array.Empty<FieldInfo>();
            }
            _fieldDefs = new FieldInfo[val.Table.Keys.Count()];
            DynValue editVal = GetOrCallMember("editDefs");
            _extraDataMap = new Dictionary<string, FieldExtraData>(_fieldDefs.Length);
            int i = 0;
            foreach (DynValue fieldKey in val.Table.Keys)
            {
                string fieldName = fieldKey.String;
                DynValue defVal = val.Table.Get(fieldKey);
                if (defVal.Type != DataType.Table) continue;
                FieldTypes fieldType = (FieldTypes)(int)_engine.GetInstanceMember(defVal, "type").Number;
                var fieldInfo = new FieldInfo
                {
                    FieldName = fieldName,
                    FieldType = fieldType,
                };
                _fieldDefs[i] = fieldInfo;
                var extra = new FieldExtraData { DefaultValue = _engine.GetInstanceMember(defVal, "default"), };
                if (editVal.Type == DataType.Table)
                {
                    DynValue tbVal = _engine.GetInstanceMember(editVal, fieldName);
                    if (!tbVal.IsNil())
                    {
                        extra.CanEdit = true;
                        var groupVal = _engine.GetInstanceMember(tbVal, "group");
                        string group = groupVal.Type == DataType.String ? groupVal.String : string.Empty;
                        var tooltipVal = _engine.GetInstanceMember(tbVal, "tooltip");
                        string tooltip = tooltipVal.Type == DataType.String ? tooltipVal.String : string.Empty;
                        var ctrlTypeVal = _engine.GetInstanceMember(tbVal, "control");
                        Type? ctrlType = ctrlTypeVal.IsNil() ? null : ctrlTypeVal.ToObject<Type>();
                        var paramVal = _engine.GetInstanceMember(tbVal, "parameter");
                        string param = paramVal.Type == DataType.String ? paramVal.String : string.Empty;
                        var validatorVal = _engine.GetInstanceMember(tbVal, "validator");
                        Type? validatorType = validatorVal.IsNil() ? null : validatorVal.ToObject<Type>();
                        var defMemberVal = _engine.GetInstanceMember(tbVal, "defaultMemberValue");
                        extra.EditGroup = group;
                        extra.Tooltip = tooltip;
                        extra.EditControlType = ctrlType;
                        extra.EditParameter = param;
                        extra.EditValidatorType = validatorType;
                        extra.DefaultMemberValue = defMemberVal;    // Here saves the DynValue directly.
                    }
                    else
                    {
                        extra.CanEdit = false;
                    }
                }
                _extraDataMap[fieldName] = extra;
                ++i;
            }
        }
        return _fieldDefs;
    }

    public object? GetDefaultValue(string fieldName)
    {
        GetDefinedFields();
        FieldInfo? fieldInfo = _fieldDefs!.FirstOrDefault(info => info.FieldName == fieldName);
        if (fieldInfo == null) throw new WorkspaceException($"Non-exist field '{fieldName}'");
        FieldExtraData extra = _extraDataMap![fieldName];
        DynValue defaultVal = extra.DefaultValue;
        return ConversionHelper.FieldTypeToValue(fieldInfo.FieldType, defaultVal);
    }

    public void FillEditingInfo(string fieldName, FieldEditInfo editInfo)
    {
        GetDefinedFields();
        if (!_extraDataMap!.TryGetValue(fieldName, out FieldExtraData? extra))
            return;
        editInfo.EditControlType = extra.EditControlType;
        editInfo.Tooltip = extra.Tooltip;
        editInfo.EditGroup = extra.EditGroup;
        editInfo.EditParameter = extra.EditParameter;
        var field = _fieldDefs!.First(f => f.FieldName == fieldName);
        if (field.FieldType >= FieldTypes.BooleanArray)
        {
            editInfo.DefaultMemberValue = ConversionHelper.FieldTypeToValue((FieldTypes)(field.FieldType - FieldTypes.BooleanArray), extra.DefaultValue);
        }
        else
        {
            editInfo.DefaultMemberValue = null;
        }
    }

    public bool CanEditField(string fieldName)
    {
        GetDefinedFields();
        if (!_extraDataMap!.TryGetValue(fieldName, out FieldExtraData? extra))
            return false;
        return extra.CanEdit;
    }

    public Type? GetFieldValidatorType(string fieldName)
    {
        GetDefinedFields();
        if (!_extraDataMap!.TryGetValue(fieldName, out FieldExtraData? extra))
            return null;
        return extra.EditValidatorType;
    }

    protected DynValue GetOrCallMember(string memberName, params object[] args)
    {
        DynValue member = _engine.GetInstanceMember(_obj, memberName);
        if (member.Type == DataType.Function)
        {
            _engine.CallInstanceFunction(_obj, memberName, out DynValue val, args);
            return val;
        }
        return _engine.GetInstanceMember(_obj, memberName);
    }

    protected ScriptEngine _engine;
    protected DynValue _obj;
    protected FieldInfo[]? _fieldDefs;
    protected Dictionary<string, FieldExtraData>? _extraDataMap;
}