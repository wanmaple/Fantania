namespace Fantania;

public class EditGroupReferenceAttribute : EditAttribute
{
    public string Group { get; set; }

    public EditGroupReferenceAttribute(string group)
    {
        Group = group;
    }
}