namespace FantaniaLib;

public interface INoise2D
{
    int Seed { get; set; }

    void TransformCoordinate(ref float x, ref float y);
    float Noise(float x, float y, int repeat);
}