using System.Collections.Generic;

namespace Fantania;

public class EditEnumAttribute : EditAttribute
{
    public HashSet<string> Excepts { get; private set; } = new HashSet<string>();

    public EditEnumAttribute(params string[] excepts)
    {
        foreach (string except in excepts)
        {
            Excepts.Add(except);
        }
    }
}