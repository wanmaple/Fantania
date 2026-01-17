
using System.Reflection;

namespace FantaniaLib;

public class SimpleEditable : SyncableObject, IEditableObject
{
    public IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var editableFields = new List<IEditableField>();
        var props = GetPropertiesWithAttribute<EditableFieldAttribute>();
        foreach (PropertyInfo prop in props)
        {
            var editableField = new SingleObjectEditableField(workspace, this, prop);
            editableFields.Add(editableField);
        }
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
    }
}