namespace FantaniaLib;

public interface IEditableField
{
    string FieldName { get; }
    FieldEditInfo EditInfo { get; }
    object FieldValue { get; set; }
    Type FieldType { get;}
    IFieldValidator? FieldValidator { get; }
    object SampleInstance { get; }
    IWorkspace Workspace { get; }
}