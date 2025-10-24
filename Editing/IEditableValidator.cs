namespace Fantania;

public interface IEditableValidator
{
    string ErrorText { get; }

    bool Validate(object val, IEditableProperty prop, out object finalVal);
}