namespace FantaniaLib;

public class EditableFieldAttribute : Attribute
{
    public string EditGroup { get; set; }
    public Type EditControlType { get; set; }
    public string TooltipKey { get; set; }
    public string EditParameter { get; set; }
    public Type FieldValidatorType { get; set; }
}