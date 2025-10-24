namespace Fantania;

public class EditAngleAttribute : EditAttribute
{
    public double MinimumValue { get; set; }
    public double MaximumValue { get; set; }

    public EditAngleAttribute(double min = -100000.0f, double max = 100000.0f)
    {
        MinimumValue = min;
        MaximumValue = max;
    }
}