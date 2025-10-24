namespace Fantania;

public abstract class UniformGradient1D : IGradient1D
{
    public float Repeat => Segment * MaxSegments;

    public float[] Values => _values;

    public float Segment { get; private set; }
    public int MaxSegments { get; private set; }

    protected UniformGradient1D(float segment, int maxSegments)
    {
        Segment = segment;
        MaxSegments = maxSegments;
        _values = new float[MaxSegments];
    }

    public Gradient1DBoundary GetBoundary(float x)
    {
        int l = MathHelper.FloorToInt(x / Segment);
        int r = (l + 1) % MaxSegments;
        return new Gradient1DBoundary
        {
            left = l * Segment,
            right = r * Segment,
        };
    }

    public float GetLeftGradient(float x)
    {
        int l = MathHelper.FloorToInt(x / Segment);
        return _values[l];
    }

    public float GetRightGradient(float x)
    {
        int l = MathHelper.FloorToInt(x / Segment);
        int r = (l + 1) % MaxSegments;
        return _values[r];
    }
    
    float[] _values;
}
