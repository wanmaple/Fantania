using System.Diagnostics.CodeAnalysis;

namespace Fantania;

public class PerlinNoiseParameters : INoiseParameters
{
    [EditEnum]
    public NoiseHelper.PerlinSmoothFunctions SmoothFunction { get; set; } = NoiseHelper.PerlinSmoothFunctions.Quintic;

    public void CopyFrom(INoiseParameters other)
    {
        if (other is PerlinNoiseParameters args)
        {
            SmoothFunction = args.SmoothFunction;
        }
    }

    public bool Equals(INoiseParameters? other)
    {
        return other is PerlinNoiseParameters args && SmoothFunction == args.SmoothFunction;
    }
}

public class PerlinNoise2D : INoise2D
{
    public int Seed { get; set; }
    public PerlinNoiseParameters Arguments { get; private set; }

    public PerlinNoise2D([NotNull] PerlinNoiseParameters args)
    {
        Arguments = args;
    }

    public void TransformCoordinate(ref float x, ref float y)
    {
    }

    public float Noise(float x, float y, int repeat)
    {
        int x0 = MathHelper.FloorToInt(x);
        int y0 = MathHelper.FloorToInt(y);
        if (repeat > 0)
        {
            x0 = ((x0 % repeat) + repeat) % repeat;
            y0 = ((y0 % repeat) + repeat) % repeat;
        }
        float xd0 = (float)(x - x0);
        float yd0 = (float)(y - y0);
        float xd1 = xd0 - 1;
        float yd1 = yd0 - 1;
        float xs = Arguments.SmoothFunction == NoiseHelper.PerlinSmoothFunctions.Hermit ? MathHelper.Hermite(xd0) : MathHelper.Quintic(xd0);
        float ys = Arguments.SmoothFunction == NoiseHelper.PerlinSmoothFunctions.Hermit ? MathHelper.Hermite(yd0) : MathHelper.Quintic(yd0);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        if (repeat > 0)
        {
            x1 = ((x1 % repeat) + repeat) % repeat;
            y1 = ((y1 % repeat) + repeat) % repeat;
        }
        x0 *= NoiseHelper.PrimeX;
        y0 *= NoiseHelper.PrimeY;
        x1 *= NoiseHelper.PrimeX;
        y1 *= NoiseHelper.PrimeY;
        float xf0 = MathHelper.Lerp(NoiseHelper.GradientCoord(Seed, x0, y0, xd0, yd0), NoiseHelper.GradientCoord(Seed, x1, y0, xd1, yd0), xs);
        float xf1 = MathHelper.Lerp(NoiseHelper.GradientCoord(Seed, x0, y1, xd0, yd1), NoiseHelper.GradientCoord(Seed, x1, y1, xd1, yd1), xs);
        return MathHelper.Lerp(xf0, xf1, ys) * 1.4247691104677813f;
    }
}
