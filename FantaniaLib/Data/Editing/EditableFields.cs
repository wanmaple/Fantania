namespace FantaniaLib;

public class EditableFields
{
    public IReadOnlyList<IEditableField> Fields => _editableFields;
    public IWorkspace Workspace { get; private set; }

    public EditableFields(IWorkspace workspace, IEnumerable<IEditableField> fields)
    {
        Workspace = workspace;
        _editableFields = fields.ToArray();
    }

    IEditableField[] _editableFields;
}