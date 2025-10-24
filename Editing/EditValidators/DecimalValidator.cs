using System;

namespace Fantania;

public class DecimalValidator : IEditableValidator
{
    public string ErrorText => _err;

    public bool Validate(object val, IEditableProperty prop, out object finalVal)
    {
        finalVal = null;
        _err = string.Empty;
        try
        {
            double number = Convert.ToDouble(val);
            EditDecimalAttribute attr = prop.EditInfo as EditDecimalAttribute;
            if (attr == null)
            {
                _err = "Develop configuration error.";
                return false;
            }
            if (number < attr.RangeMinimum || number > attr.RangeMaximum)
            {
                _err = string.Format(Localization.Resources.ValidationErrorDecimalRange, attr.RangeMinimum, attr.RangeMaximum);
                return false;
            }
            finalVal = number;
            return true;
        }
        catch (FormatException)
        {
            _err = Localization.Resources.ValidationErrorDecimalFormat;
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