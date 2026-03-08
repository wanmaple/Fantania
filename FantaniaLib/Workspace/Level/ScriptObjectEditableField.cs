using System.ComponentModel;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class ScriptObjectEditableField : ObservableObject, IEditableField
{
    public string FieldName => _fieldName;
    public FieldEditInfo EditInfo => _editInfo;
    public object FieldValue 
    { 
        get => _scriptObj.GetFieldValue(FieldName)!;
        set
        {
            if (value == null) return;
            if (!FieldValue.Equals(value))
            {
                if (FieldValidator == null || FieldValidator.ValidateField(Workspace, value))
                {
                    _scriptObj.SetFieldValue(FieldName, value);
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
            else if (FieldValidator != null)
                FieldValidator.ValidateField(Workspace, value);
        }
    }
    public Type FieldType => FieldValue.GetType();
    public IFieldValidator? FieldValidator => _validator;
    public object SampleInstance => _scriptObj;
    public IWorkspace Workspace { get; private set; }

    public ScriptObjectEditableField(IWorkspace workspace, ScriptObject placement, string fieldName)
    {
        Workspace = workspace;
        _scriptObj = placement;
        _fieldName = fieldName;

        _scriptObj.Template.FillEditingInfo(_fieldName, _editInfo);
        Type? validatorType = _scriptObj.Template.GetFieldValidatorType(_fieldName);
        if (validatorType != null)
            _validator = Activator.CreateInstance(validatorType) as IFieldValidator;
        if (_validator == null)
            _validator = EmptyFieldValidator.Empty;
        _validator.ValidateField(Workspace, FieldValue);

        WeakEventHandlerManager.Subscribe<ScriptObject, PropertyChangedEventArgs, ScriptObjectEditableField>(_scriptObj, nameof(PropertyChanged), OnPlacementPropertyChanged);
    }

    ~ScriptObjectEditableField()
    {
        WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, ScriptObjectEditableField>(_scriptObj, nameof(PropertyChanged), OnPlacementPropertyChanged);
    }

    void OnPlacementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _fieldName)
        {
            OnPropertyChanged(nameof(FieldValue));
        }
    }

    ScriptObject _scriptObj;
    FieldEditInfo _editInfo = new FieldEditInfo();
    string _fieldName;
    IFieldValidator? _validator;
}