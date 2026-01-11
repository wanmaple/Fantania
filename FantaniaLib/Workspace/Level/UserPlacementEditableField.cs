using System.ComponentModel;
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
                if (FieldValidator == null || FieldValidator.ValidateField(value))
                {
                    _placement.SetFieldValue(FieldName, value);
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
        }
    }
    public IFieldValidator? FieldValidator => _validator;

    public UserPlacementEditableField(UserPlacement placement, string fieldName)
    {
        _placement = placement;
        _fieldName = fieldName;

        _placement.Template.FillEditingInfo(_fieldName, _editInfo);
        Type? validatorType = _placement.Template.GetFieldValidatorType(_fieldName);
        if (validatorType != null)
            _validator = Activator.CreateInstance(validatorType) as IFieldValidator;
        if (_validator == null)
            _validator = EmptyFieldValidator.Empty;

        _placement.PropertyChanged += OnPlacementPropertyChanged;
    }

    ~UserPlacementEditableField()
    {
        _placement.PropertyChanged -= OnPlacementPropertyChanged;
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