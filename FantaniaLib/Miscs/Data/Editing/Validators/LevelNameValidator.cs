using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class LevelNameValidator : ObservableObject, IFieldValidator
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

    public bool ValidateField(IWorkspace workspace, object fieldValue)
    {
        if (_nameValidator.ValidateField(workspace, fieldValue))
        {
            string name = (string)fieldValue;
            if (workspace.LevelModule.LevelDescriptions.Any(desc => desc.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Error = string.Format(workspace.LocalizeString("ERR_DuplicatedLevelName"), name);
                return false;
            }
            Error = string.Empty;
            return true;
        }
        Error = _nameValidator.Error;
        return false;
    }

    SimpleNameValidator _nameValidator = new SimpleNameValidator();
}