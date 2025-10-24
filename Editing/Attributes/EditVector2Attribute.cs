namespace Fantania;

public class EditVector2Attribute : EditAttribute
{
    public float XMinimum { get; set; } = -100000.0f;
    public float XMaximum { get; set; } = 100000.0f;
    public float YMinimum { get; set; } = -100000.0f;
    public float YMaximum { get; set; } = 100000.0f;
    public float Increment { get; set; } = 1.0f;

    public EditVector2Attribute()
    {
    }
}