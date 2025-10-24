using System;

namespace Fantania;

[AttributeUsage(AttributeTargets.Property)]
public class RenameColumnAttribute : Attribute
{
    public string OldName { get; set; }

    public RenameColumnAttribute(string oldName)
    {
        OldName = oldName;
    }
}