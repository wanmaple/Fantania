using System.ComponentModel;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class UserGameDataEditableField : ObservableObject, IEditableField
{
    public string FieldName => _fieldName;
    public FieldEditInfo EditInfo => _editInfo;
    public object FieldValue 
    { 
        get => _gamedata.GetFieldValue(FieldName)!;
        set
        {
            if (value == null) return;
            if (!FieldValue.Equals(value))
            {
                if (FieldValidator == null || FieldValidator.ValidateField(Workspace, value))
                {
                    _gamedata.SetFieldValue(FieldName, value);
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
            else if (FieldValidator != null)
                FieldValidator.ValidateField(Workspace, value);
        }
    }
    public Type FieldType => FieldValue.GetType();
    public IFieldValidator? FieldValidator => _validator;
    public object SampleInstance => _gamedata;
    public IWorkspace Workspace { get; private set; }

    public UserGameDataEditableField(IWorkspace workspace, UserGameData userGameData, string fieldName)
    {
        Workspace = workspace;
        _gamedata = userGameData;
        _fieldName = fieldName;

        _gamedata.Template.FillEditingInfo(_fieldName, _editInfo);
        Type? validatorType = _gamedata.Template.GetFieldValidatorType(_fieldName);
        if (validatorType != null)
            _validator = Activator.CreateInstance(validatorType) as IFieldValidator;
        if (_validator == null)
            _validator = EmptyFieldValidator.Empty;
        _validator.ValidateField(Workspace, FieldValue);

        WeakEventHandlerManager.Subscribe<UserGameData, PropertyChangedEventArgs, UserGameDataEditableField>(_gamedata, nameof(PropertyChanged), OnUserGameDataPropertyChanged);
    }

    ~UserGameDataEditableField()
    {
        WeakEventHandlerManager.Unsubscribe<PropertyChangedEventArgs, UserGameDataEditableField>(_gamedata, nameof(PropertyChanged), OnUserGameDataPropertyChanged);
    }

    void OnUserGameDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _fieldName)
        {
            OnPropertyChanged(nameof(FieldValue));
        }
    }

    UserGameData _gamedata;
    FieldEditInfo _editInfo = new FieldEditInfo();
    string _fieldName;
    IFieldValidator? _validator;
}