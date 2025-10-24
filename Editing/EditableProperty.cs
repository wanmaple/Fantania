using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Fantania;

public class EditableProperty : ObservableObject, IEditableProperty
{
    public event Action<object> ValueChanged;

    public PropertyInfo PropertyInfo => _propertyInfo;
    public object Instance => _instance;
    public string PropertyName => _propertyInfo.Name;
    public EditAttribute EditInfo { get; set; }
    public Type EditType => EditInfo.GetType();
    public IEditableValidator Validator { get; set; }
    public string Tooltip => _tooltip;

    public object EditValue
    {
        get => GetValue(_instance);
        set
        {
            object oldValue = GetValue(_instance);
            if (oldValue != value)
            {
                object finalValue = null;
                bool validationPass = Validator == null || Validator.Validate(value, this, out finalValue);
                if (validationPass)
                {
                    Error = null;
                    SetValue(_instance, finalValue ?? value);
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

    public EditableProperty(ObservableObject instance, PropertyInfo propInfo, EditAttribute editInfo, string tooltip = "")
    {
        _instance = instance;
        _propertyInfo = propInfo;
        EditInfo = editInfo;
        _tooltip = tooltip;
        _instance.PropertyChanged += OnInstancePropertyChanged;
        SetupValidator();
    }

    public object GetValue(object instance)
    {
        return _propertyInfo.GetValue(instance);
    }

    public void SetValue(object instance, object value)
    {
        _propertyInfo.SetValue(instance, value);
    }

    ~EditableProperty()
    {
        _instance.PropertyChanged -= OnInstancePropertyChanged;
    }

    void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == PropertyName)
        {
            OnPropertyChanged(nameof(EditValue));
            ValueChanged?.Invoke(EditValue);
        }
    }

    void SetupValidator()
    {
        if (ReflectionHelper.IsArrayType(PropertyInfo.PropertyType)) return;
        if (EDITABLE_VALIDATOR_MAPPING.TryGetValue(EditType, out IEditableValidator validator))
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

    public void RegisterPropertyChanged(Action<object> handler)
    {
        _handlers.Add(handler);
        _instance.PropertyChanged += OnPropertyChanged;
    }

    public void UnRegisterPropertyChanged(Action<object> handler)
    {
        _instance.PropertyChanged -= OnPropertyChanged;
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

    void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(EditValue));
    }

    PropertyInfo _propertyInfo;
    ObservableObject _instance;
    string _tooltip;
    List<Action<object>> _handlers = new List<Action<object>>(0);

    internal static readonly Dictionary<Type, IEditableValidator> EDITABLE_VALIDATOR_MAPPING = new Dictionary<Type, IEditableValidator>
    {
        { typeof(EditIntegerAttribute), new IntegerValidator() },
        { typeof(EditDecimalAttribute), new DecimalValidator() },
        { typeof(EditNameAttribute), new NameValidator() },
    };
}