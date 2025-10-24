namespace Fantania;

public class EditDecimalAttribute : EditAttribute
{
    public double RangeMinimum { get; set; }
    public double RangeMaximum { get; set; }

    public EditDecimalAttribute(double rangeMin = -100000.0f, double rangeMax = 100000.0f)
    {
        RangeMinimum = rangeMin;
        RangeMaximum = rangeMax;
    }
}