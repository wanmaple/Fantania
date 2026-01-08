namespace FantaniaLib;

public class EditableFields
{
    public IReadOnlyList<IEditableField> Fields => _editableFields;

    public EditableFields(IEnumerable<IEditableField> fields)
    {
        _editableFields = fields.ToArray();
    }

    IEditableField[] _editableFields;
}