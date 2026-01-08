namespace FantaniaLib;

public interface IEditableObject
{
    IReadOnlyList<IEditableField> EditableFields { get; }
}