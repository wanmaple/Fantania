namespace FantaniaLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum)]
public class BindingScriptAttribute : Attribute
{
    public string CustomName { get; set; } = null;
}