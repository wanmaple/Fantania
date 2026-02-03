using System.Text.RegularExpressions;

namespace FantaniaLib;

public class SimpleNameValidator : FieldValidatorBase
{
    public override bool ValidateField(IWorkspace workspace, object fieldValue)
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