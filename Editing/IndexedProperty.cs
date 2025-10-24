using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class IndexedProperty : ObservableObject, IEditableProperty
{
    public string PropertyName => _index.ToString();
    public object Instance => _instance;
    public PropertyInfo PropertyInfo => _indexerInfo;
    public EditAttribute EditInfo => _editInfo;
    public Type EditType => _editInfo.GetType();
    public IEditableValidator Validator { get; private set; }
    public string Tooltip => string.Empty;
    public int Index => _index;

    public object EditValue
    {
        get
        {
            return GetValue();
        }
        set
        {
            object oldValue = EditValue;
            if (oldValue != value)
            {
                object finalValue = null;
                bool validationPass = Validator == null || Validator.Validate(value, this, out finalValue);
                if (validationPass)
                {
                    Error = null;
                    SetValue(finalValue ?? value);
                    OnPropertyChanged(nameof(EditValue));
                }
                else
                    Error = Validator.ErrorText;
            }
        }
    }
    public string Error { get; set; }

    public event Action<object> ValueChanged;

    public IndexedProperty(object instance, int index, PropertyInfo propInfo, EditAttribute editInfo)
    {
        _instance = instance;
        _index = index;
        _indexerInfo = propInfo.PropertyType.GetProperty("Item");
        _editInfo = editInfo;
        SetupValidator();
    }

    public void RegisterPropertyChanged(Action<object> handler)
    {
    }

    public void UnRegisterPropertyChanged(Action<object> handler)
    {
    }

    object GetValue()
    {
        return _indexerInfo.GetValue(_instance, [_index,]);
    }

    void SetValue(object val)
    {
        _indexerInfo.SetValue(_instance, val, [_index,]);
    }

    void SetupValidator()
    {
        if (EditableProperty.EDITABLE_VALIDATOR_MAPPING.TryGetValue(EditType, out IEditableValidator validator))
        {
            Validator = validator;
        }
        object finalValue = null;
        bool validationPass = Validator == null || Validator.Validate(EditValue, this, out finalValue);
        if (validationPass)
            Error = null;
        else
            Error = Validator.ErrorText;
    }

    object _instance;
    int _index;
    PropertyInfo _indexerInfo;
    EditAttribute _editInfo;
}