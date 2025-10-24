using System;

namespace Fantania;

public class EditIntegerAttribute : EditAttribute
{
    public int RangeMinimum { get; set; }
    public int RangeMaximum { get; set; }

    public EditIntegerAttribute(int rangeMin = int.MinValue, int rangeMax = int.MaxValue)
    {
        RangeMinimum = rangeMin;
        RangeMaximum = rangeMax;
    }
}