namespace FantaniaLib;

public class EmptyFieldValidator : IFieldValidator
{
    public static readonly IFieldValidator Empty = new EmptyFieldValidator();

    public string Error => string.Empty;

    public bool ValidateField(object fieldValue)
    {
        return true;
    }
}