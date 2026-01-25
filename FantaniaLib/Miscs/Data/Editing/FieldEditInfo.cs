namespace FantaniaLib;

public class FieldEditInfo
{
    public string EditGroup { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public Type? EditControlType { get; set; }
    public string EditParameter { get; set; } = string.Empty;
}