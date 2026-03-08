namespace FantaniaLib;

public class FieldEditInfo
{
    public string EditGroup { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public Type? EditControlType { get; set; }
    public string EditParameter { get; set; } = string.Empty;
    public object? DefaultMemberValue { get; set; }   // Only used for array-member field defaults, ignored otherwise.

    public override bool Equals(object? obj)
    {
        return obj is FieldEditInfo info &&
               EditGroup == info.EditGroup &&
               Tooltip == info.Tooltip &&
               EqualityComparer<Type?>.Default.Equals(EditControlType, info.EditControlType) &&
               EditParameter == info.EditParameter &&
               EqualityComparer<object?>.Default.Equals(DefaultMemberValue, info.DefaultMemberValue);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EditGroup, Tooltip, EditControlType, EditParameter, DefaultMemberValue);
    }
}