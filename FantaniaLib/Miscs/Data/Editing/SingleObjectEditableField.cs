using System.ComponentModel;
using System.Reflection;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class SingleObjectEditableField : ObservableObject, IEditableField
{
    public string FieldName => _propInfo.Name;
    public FieldEditInfo EditInfo => _editInfo;
    public object FieldValue
    {
        get => _propInfo.GetValue(_instance)!;
        set
        {
            if (!FieldValue.Equals(value))
            {
                if (FieldValidator == null || FieldValidator.ValidateField(Workspace, value))
                {
                    _propInfo.SetValue(_instance, value);
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
            else if (FieldValidator != null)
                FieldValidator.ValidateField(Workspace, value);
        } 
    }
    public IFieldValidator? FieldValidator => _validator;
    public IWorkspace Workspace { get; private set; }

    public SingleObjectEditableField(IWorkspace workspace, ObservableObject instance, PropertyInfo propInfo)
    {
        Workspace = workspace;
        _instance = instance;
        _propInfo = propInfo;
        
        EditableFieldAttribute attr = propInfo.GetCustomAttribute<EditableFieldAttribute>()!;
        _editInfo.EditGroup = attr.EditGroup;
        _editInfo.Tooltip = attr.TooltipKey;
        _editInfo.EditControlType = attr.EditControlType;
        _editInfo.EditParameter = attr.EditParameter;
        if (attr.FieldValidatorType != null)
            _validator = Activator.CreateInstance(attr.FieldValidatorType) as IFieldValidator;
        if (_validator == null)
            _validator = EmptyFieldValidator.Empty;
        _validator.ValidateField(Workspace, FieldValue);

        WeakEventHandlerManager.Subscribe<ObservableObject, PropertyChangedEventArgs, SingleObjectEditableField>(_instance, nameof(PropertyChanged), OnInstancePropertyChanged);
    }

    ~SingleObjectEditableField()
    {
        WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, SingleObjectEditableField>(_instance, nameof(PropertyChanged), OnInstancePropertyChanged);
    }

    void OnInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == FieldName)
        {
            OnPropertyChanged(nameof(FieldValue));
        }
    }

    ObservableObject _instance;
    PropertyInfo _propInfo;
    FieldEditInfo _editInfo = new FieldEditInfo();
    IFieldValidator? _validator;
}