namespace FantaniaLib;

public class EditableFieldAttribute : Attribute
{
    public string EditGroup { get; set; } = string.Empty;
    public Type? EditControlType { get; set; }
    public string TooltipKey { get; set; } = string.Empty;
    public string EditParameter { get; set; } = string.Empty;
    public Type? FieldValidatorType { get; set; }
}