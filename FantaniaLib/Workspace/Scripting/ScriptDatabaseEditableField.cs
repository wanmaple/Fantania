using System.ComponentModel;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class ScriptDatabaseEditableField : ObservableObject, IEditableField
{
    public string FieldName => _fieldName;
    public FieldEditInfo EditInfo => _editInfo;
    public object FieldValue
    {
        get => _obj.GetFieldValue(FieldName)!;
        set
        {
            if (value == null) return;
            if (!FieldValue.Equals(value))
            {
                FieldValidator?.ValidateField(Workspace, value);
                _obj.SetFieldValue(FieldName, value);
                OnPropertyChanged(nameof(FieldValue));
            }
            else if (FieldValidator != null)
                FieldValidator.ValidateField(Workspace, value);
        }
    }
    public Type FieldType => FieldValue.GetType();
    public IFieldValidator? FieldValidator => _validator;
    public object SampleInstance => _obj;
    public IWorkspace Workspace { get; private set; }

    public ScriptDatabaseEditableField(IWorkspace workspace, ScriptDatabaseObject obj, string fieldName)
    {
        Workspace = workspace;
        _obj = obj;
        _fieldName = fieldName;

        _obj.Template.FillEditingInfo(_fieldName, _editInfo);
        Type? validatorType = _obj.Template.GetFieldValidatorType(_fieldName);
        if (validatorType != null)
            _validator = Activator.CreateInstance(validatorType) as IFieldValidator;
        if (_validator == null)
            _validator = EmptyFieldValidator.Empty;
        _validator.ValidateField(Workspace, FieldValue);

        WeakEventHandlerManager.Subscribe<ScriptDatabaseObject, PropertyChangedEventArgs, ScriptDatabaseEditableField>(_obj, nameof(PropertyChanged), OnObjectPropertyChanged);
    }

    ~ScriptDatabaseEditableField()
    {
        WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, ScriptDatabaseEditableField>(_obj, nameof(PropertyChanged), OnObjectPropertyChanged);
    }

    void OnObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _fieldName)
        {
            OnPropertyChanged(nameof(FieldValue));
        }
    }

    ScriptDatabaseObject _obj;
    FieldEditInfo _editInfo = new FieldEditInfo();
    string _fieldName;
    IFieldValidator? _validator;
}