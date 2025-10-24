using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class TempEditableProperty : ObservableObject, IEditableProperty
{
    public event Action<object> ValueChanged;

    public PropertyInfo PropertyInfo => _propertyInfo;
    public object Instance => _instance;
    public string PropertyName => _propertyInfo.Name;
    public EditAttribute EditInfo { get; set; }
    public Type EditType => EditInfo.GetType();
    public IEditableValidator Validator { get; set; }
    public string Tooltip => string.Empty;
    public string Error { get; set; }

    public object EditValue
    {
        get => GetValue(_instance);
        set
        {
            object oldValue = GetValue(_instance);
            if (!oldValue.Equals(value))
            {
                SetValue(_instance, value);
                OnPropertyChanged(nameof(EditValue));
                ValueChanged?.Invoke(value);
            }
        }
    }

    public TempEditableProperty(object instance, PropertyInfo propInfo, EditAttribute editInfo)
    {
        _instance = instance;
        _propertyInfo = propInfo;
        EditInfo = editInfo;
    }

    public object GetValue(object instance)
    {
        return _propertyInfo.GetValue(instance);
    }

    public void SetValue(object instance, object value)
    {
        _propertyInfo.SetValue(instance, value);
    }

    public void RegisterPropertyChanged(Action<object> handler)
    {
    }

    public void UnRegisterPropertyChanged(Action<object> handler)
    {
    }

    PropertyInfo _propertyInfo;
    object _instance;
}
