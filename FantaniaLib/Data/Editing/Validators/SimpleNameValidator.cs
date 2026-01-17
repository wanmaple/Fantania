using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FantaniaLib;

public class SimpleNameValidator : ObservableObject, IFieldValidator
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
        string name = (string)fieldValue;
        if (!Regex.IsMatch(name, NAME_REGEX))
        {
            Error = workspace.LocalizeString("ERR_InvalidLevelName");
            return false;
        }
        Error = string.Empty;
        return true;
    }

    string NAME_REGEX = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
}