namespace Fantania;

public interface INoise1D
{
    float Get(float x, int octaves = 1, float persistency = 1.0f);
}