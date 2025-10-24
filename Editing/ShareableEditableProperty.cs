using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class ShareableEditableProperty : ObservableObject, IEditableProperty
{
    public event Action<object> ValueChanged;

    public string PropertyName => _propertyInfo.PropertyName;
    public object Instance => _instances.First();
    public PropertyInfo PropertyInfo => _propertyInfo.PropertyInfo;
    public EditAttribute EditInfo => _propertyInfo.EditInfo;
    public Type EditType => EditInfo.GetType();
    public IEditableValidator Validator => _propertyInfo.Validator;
    public string Tooltip => _propertyInfo.Tooltip;

    public object EditValue
    {
        get
        {
            return _propertyInfo.GetValue(_instances.First());
        }
        set
        {
            object oldValue = _propertyInfo.GetValue(_instances.First());
            if (oldValue != value)
            {
                object finalValue = null;
                bool validationPass = Validator == null || Validator.Validate(value, _propertyInfo, out finalValue);
                if (validationPass)
                {
                    Error = null;
                    foreach (var instance in _instances)
                    {
                        _propertyInfo.SetValue(instance, finalValue ?? value);
                    }
                    OnPropertyChanged(nameof(EditValue));
                }
                else
                    Error = Validator.ErrorText;
            }
        }
    }

    private string _error;
    public string Error
    {
        get { return _error; }
        set
        {
            if (_error != value)
            {
                _error = value;
                OnPropertyChanged(nameof(Error));
            }
        }
    }

    public ShareableEditableProperty(EditableProperty prop, IEnumerable<ObservableObject> instances)
    {
        _propertyInfo = prop;
        _instances = instances;
        foreach (var instance in _instances)
        {
            instance.PropertyChanged += OnInstancePropertyChanged;
        }
    }

    ~ShareableEditableProperty()
    {
        foreach (var instance in _instances)
        {
            instance.PropertyChanged -= OnInstancePropertyChanged;
        }
    }

    void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == PropertyName)
        {
            OnPropertyChanged(nameof(EditValue));
            ValueChanged?.Invoke(EditValue);
        }
    }

    public void RegisterPropertyChanged(Action<object> handler)
    {
        _handlers.Add(handler);
        _instances.First().PropertyChanged += OnPropertyChanged;
    }

    public void UnRegisterPropertyChanged(Action<object> handler)
    {
        _instances.First().PropertyChanged -= OnPropertyChanged;
        _handlers.Remove(handler);
    }

    void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == PropertyName)
        {
            foreach (var handler in _handlers)
            {
                handler.Invoke(EditValue);
            }
        }
    }

    List<Action<object>> _handlers = new List<Action<object>>(0);

    EditableProperty _propertyInfo;
    IEnumerable<ObservableObject> _instances;
}