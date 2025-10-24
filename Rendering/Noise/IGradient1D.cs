namespace Fantania;

public struct Gradient1DBoundary
{
    public float Size => right - left;

    public float left;
    public float right;
}

public interface IGradient1D
{
    float Repeat { get; }

    float GetLeftGradient(float x);
    float GetRightGradient(float x);
    Gradient1DBoundary GetBoundary(float x);
}