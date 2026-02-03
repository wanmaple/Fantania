using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public abstract class FieldValidatorBase : ObservableObject, IFieldValidator
{
    private string _err = string.Empty;
    public string Error
    {
        get { return _err; }
        set
        {
            if (_err != value)
            {
                _err = value;
                OnPropertyChanged(nameof(Error));
            }
        }
    }

    public abstract bool ValidateField(IWorkspace workspace, object fieldValue);
}