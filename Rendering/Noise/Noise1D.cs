namespace Fantania;

public class Noise1D : INoise1D
{
    public IGradient1D Gradient { get; private set; }

    public Noise1D(IGradient1D gradient)
    {
        Gradient = gradient;
    }

    /// <summary>
    /// 多个频率叠加后的噪声(用来模拟更加真实的自然噪声)
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="segSize">单格大小</param>
    /// <param name="octaves">叠加多少个相差八度的噪声</param>
    /// <param name="persistency">趋向</param>
    /// <returns></returns>
    public float Get(float x, int octaves = 1, float persistency = 1.0f)
    {
        float total = 0.0f;
        float frequency = 1.0f;
        float amplitude = 1.0f;
        float max = 0.0f;
        for (int i = 0; i < octaves; i++)
        {
            total += Noise(x * frequency);
            max += amplitude;
            amplitude *= persistency;
            frequency *= 2.0f;
        }
        return total / max;
    }

    float Noise(float x)
    {
        if (Gradient.Repeat > 0.0f)
        {
            x = x % Gradient.Repeat;
        }
        float gradLeft = Gradient.GetLeftGradient(x);
        float gradRight = Gradient.GetRightGradient(x);
        Gradient1DBoundary boundary = Gradient.GetBoundary(x);
        float u = (x - boundary.left) / boundary.Size;
        u = MathHelper.Quintic(u);
        return MathHelper.Lerp(gradLeft, gradRight, u);
    }
}