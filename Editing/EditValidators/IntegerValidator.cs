using System;

namespace Fantania;

public class IntegerValidator : IEditableValidator
{
    public string ErrorText => _err;

    public bool Validate(object val, IEditableProperty prop, out object finalVal)
    {
        finalVal = null;
        _err = string.Empty;
        try
        {
            int integer = Convert.ToInt32(val);
            EditIntegerAttribute attr = prop.EditInfo as EditIntegerAttribute;
            if (attr == null)
            {
                _err = "Develop configuration error.";
                return false;
            }
            if (integer < attr.RangeMinimum || integer > attr.RangeMaximum)
            {
                _err = string.Format(Localization.Resources.ValidationErrorIntegerRange, attr.RangeMinimum, attr.RangeMaximum);
                return false;
            }
            finalVal = integer;
            return true;
        }
        catch (FormatException)
        {
            _err = Localization.Resources.ValidationErrorIntegerFormat;
            return false;
        }
        catch (Exception ex)
        {
            _err = string.Format(Localization.Resources.ValidationErrorUnknown, $"{ex.GetType()}\nMessage: {ex.Message}");
            return false;
        }
    }

    string _err = string.Empty;
}