namespace FantaniaLib;

public interface IEditableObject
{
    IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace);
}