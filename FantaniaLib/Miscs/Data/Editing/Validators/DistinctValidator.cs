namespace FantaniaLib;

public class DistinctValidator : FieldValidatorBase
{
    public override bool ValidateField(IWorkspace workspace, object fieldValue)
    {
        _set.Clear();
        Type type = fieldValue.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FantaniaArray<>))
        {
            foreach (object item in (System.Collections.IEnumerable)fieldValue)
            {
                if (!_set.Add(item))
                {
                    Error = workspace.LocalizeString("ERR_DuplicateItem");
                    return false;
                }
            }
        }
        Error = string.Empty;
        return true;
    }

    HashSet<object> _set = new HashSet<object>();
}