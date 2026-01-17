namespace FantaniaLib;

public interface IFieldValidator
{
    string Error { get; }

    bool ValidateField(IWorkspace workspace, object fieldValue);
}