namespace FantaniaLib;

public class GroupedEditableFields
{
    public required IWorkspace Workspace { get; set; }
    public required string Group { get; set; }
    public required EditableFields Fields { get; set; }
}