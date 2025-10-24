using Fantania.ViewModels;

namespace Fantania;

public class NameValidator : IEditableValidator
{
    public string ErrorText => _err;

    public bool Validate(object val, IEditableProperty prop, out object finalVal)
    {
        finalVal = val;
        _err = string.Empty;
        string name = (val as string).Trim();
        finalVal = name;
        if (string.IsNullOrEmpty(name))
        {
            _err = Localization.Resources.ValidationErrorEmptyName;
            return false;
        }
        DatabaseObject obj = prop.Instance as DatabaseObject;
        var currentObjs = WorkspaceViewModel.Current.Workspace.MainDatabase.ObjectsOfGroup(obj.Group);
        foreach (DatabaseObject exist in currentObjs)
        {
            if (prop.Instance == exist) continue;
            if (exist.Name == name)
            {
                _err = Localization.Resources.ValidationErrorDuplicatedName;
                return false;
            }
        }
        return true;
    }

    string _err = string.Empty;
}