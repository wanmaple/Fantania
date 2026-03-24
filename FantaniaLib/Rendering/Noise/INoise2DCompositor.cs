namespace FantaniaLib;

public interface INoise2DCompositor
{
    float Composite(INoise2D noise, float x, float y, int repeat);
}

public class EmptyNoiseCompositor : INoise2DCompositor
{
    public float Composite(INoise2D noise, float x, float y, int repeat)
    {
        return noise.Noise(x, y, repeat);
    }
}