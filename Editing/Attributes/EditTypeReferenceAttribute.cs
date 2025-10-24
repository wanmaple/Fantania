using System;

namespace Fantania;

public class EditTypeReferenceAttribute : EditAttribute
{
    public Type Type { get; set; }

    public EditTypeReferenceAttribute(Type type)
    {
        Type = type;
    }
}