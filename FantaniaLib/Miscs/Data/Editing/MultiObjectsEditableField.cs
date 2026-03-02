using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class MultiObjectsEditableField : ObservableObject, IEditableField
{
    public string FieldName => _fields[0].FieldName;
    public FieldEditInfo EditInfo => _fields[0].EditInfo;
    public object FieldValue
    {
        get => _fields[0].FieldValue;
        set
        {
            if (value == null) return;
            if (!FieldValue.Equals(value))
            {
                if (FieldValidator == null || FieldValidator.ValidateField(Workspace, value))
                {
                    foreach (var field in _fields)
                    {
                        field.FieldValue = value;
                    }
                    OnPropertyChanged(nameof(FieldValue));
                }
            }
        }
    }
    public Type FieldType => FieldValue.GetType();
    public IFieldValidator? FieldValidator => _fields[0].FieldValidator;
    public IWorkspace Workspace => _workspace;

    public MultiObjectsEditableField(IWorkspace workspace, IReadOnlyList<IEditableField> fields)
    {
        _fields = fields;
        _workspace = workspace;
    }

    IReadOnlyList<IEditableField> _fields;
    IWorkspace _workspace;
}