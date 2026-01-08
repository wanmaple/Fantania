namespace FantaniaLib;

public interface IFieldValidator
{
    string Error { get; }

    bool ValidateField(object fieldValue);
}