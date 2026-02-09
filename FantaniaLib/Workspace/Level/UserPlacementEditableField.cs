using System.ComponentModel;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class UserPlacementEditableField : ObservableObject, IEditableField
{
    public string FieldName => _fieldName;
    public FieldEditInfo EditInfo => _editInfo;
    public object FieldValue 
    { 
        get => _placement.GetFieldValue(FieldName)!;
        set
        {
            if (!FieldValue.Equals(value))
            {
                if (FieldValidator == null || FieldValidator.ValidateField(Workspace, value))
                {
                    _placement.SetFieldValue(FieldName, value);
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
            else if (FieldValidator != null)
                FieldValidator.ValidateField(Workspace, value);
        }
    }
    public IFieldValidator? FieldValidator => _validator;
    public IWorkspace Workspace { get; private set; }

    public UserPlacementEditableField(IWorkspace workspace, UserPlacement placement, string fieldName)
    {
        Workspace = workspace;
        _placement = placement;
        _fieldName = fieldName;

        _placement.Template.FillEditingInfo(_fieldName, _editInfo);
        Type? validatorType = _placement.Template.GetFieldValidatorType(_fieldName);
        if (validatorType != null)
            _validator = Activator.CreateInstance(validatorType) as IFieldValidator;
        if (_validator == null)
            _validator = EmptyFieldValidator.Empty;
        _validator.ValidateField(Workspace, FieldValue);

        WeakEventHandlerManager.Subscribe<UserPlacement, PropertyChangedEventArgs, UserPlacementEditableField>(_placement, nameof(PropertyChanged), OnPlacementPropertyChanged);
    }

    ~UserPlacementEditableField()
    {
        WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, UserPlacementEditableField>(_placement, nameof(PropertyChanged), OnPlacementPropertyChanged);
    }

    void OnPlacementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _fieldName)
        {
            OnPropertyChanged(nameof(FieldValue));
        }
    }

    UserPlacement _placement;
    FieldEditInfo _editInfo = new FieldEditInfo();
    string _fieldName;
    IFieldValidator? _validator;
}