namespace FantaniaLib;

public interface IEditableField
{
    public string FieldName { get; }
    FieldEditInfo EditInfo { get; }
    object FieldValue { get; set; }
    IFieldValidator FieldValidator { get; }
}