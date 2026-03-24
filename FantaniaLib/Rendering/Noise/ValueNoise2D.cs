namespace FantaniaLib;

public class ValueNoise2DParameters : NoiseParameters
{
    private NoiseHelper.ValueAlgorithms _algorithm = NoiseHelper.ValueAlgorithms.Value;
    [EditableField(TooltipKey = "TT_ValueAlgorithm")]
    public NoiseHelper.ValueAlgorithms Algorithm
    {
        get { return _algorithm; }
        set
        {
            if (_algorithm != value)
            {
                _algorithm = value;
                OnPropertyChanged(nameof(Algorithm));
            }
        }
    }
}

public class ValueNoise2D : INoise2D
{
    public int Seed { get; set; }
    public ValueNoise2DParameters Arguments { get; private set; }

    public ValueNoise2D(ValueNoise2DParameters args)
    {
        Arguments = args;
    }

    public float Noise(float x, float y, int repeat)
    {
        switch (Arguments.Algorithm)
        {
            case NoiseHelper.ValueAlgorithms.ValueCubic:
                return ValueCubic(x, y, repeat);
            case NoiseHelper.ValueAlgorithms.Value:
            default:
                return Value(x, y, repeat);
        }
    }

    float Value(float x, float y, int repeat)
    {
        int x0 = MathHelper.FloorToInt(x);
        int y0 = MathHelper.FloorToInt(y);
        if (repeat > 0)
        {
            x0 = ((x0 % repeat) + repeat) % repeat;
            y0 = ((y0 % repeat) + repeat) % repeat;
        }
        float xs = MathHelper.Hermite(x - x0);
        float ys = MathHelper.Hermite(y - y0);
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
        float xf0 = MathHelper.Lerp(NoiseHelper.ValueCoord(Seed, x0, y0), NoiseHelper.ValueCoord(Seed, x1, y0), xs);
        float xf1 = MathHelper.Lerp(NoiseHelper.ValueCoord(Seed, x0, y1), NoiseHelper.ValueCoord(Seed, x1, y1), xs);
        return MathHelper.Lerp(xf0, xf1, ys);
    }

    float ValueCubic(float x, float y, int repeat)
    {
        int x1 = MathHelper.FloorToInt(x);
        int y1 = MathHelper.FloorToInt(y);
        if (repeat > 0)
        {
            x1 = ((x1 % repeat) + repeat) % repeat;
            y1 = ((y1 % repeat) + repeat) % repeat;
        }
        float xs = x - x1;
        float ys = y - y1;
        int x0 = x1 - 1;
        int y0 = y1 - 1;
        int x2 = x1 + 1;
        int y2 = y1 + 1;
        int x3 = x1 + 2;
        int y3 = y1 + 2;
        if (repeat > 0)
        {
            x0 = ((x0 % repeat) + repeat) % repeat;
            y0 = ((y0 % repeat) + repeat) % repeat;
            x2 = ((x2 % repeat) + repeat) % repeat;
            y2 = ((y2 % repeat) + repeat) % repeat;
            x3 = ((x3 % repeat) + repeat) % repeat;
            y3 = ((y3 % repeat) + repeat) % repeat;
        }
        x1 *= NoiseHelper.PrimeX;
        y1 *= NoiseHelper.PrimeY;
        x0 *= NoiseHelper.PrimeX;
        y0 *= NoiseHelper.PrimeY;
        x2 *= NoiseHelper.PrimeX;
        y2 *= NoiseHelper.PrimeY;
        x3 *= NoiseHelper.PrimeX;
        y3 *= NoiseHelper.PrimeY;
        return MathHelper.CubicLerp(
            MathHelper.CubicLerp(NoiseHelper.ValueCoord(Seed, x0, y0), NoiseHelper.ValueCoord(Seed, x1, y0), NoiseHelper.ValueCoord(Seed, x2, y0), NoiseHelper.ValueCoord(Seed, x3, y0),
                      xs),
            MathHelper.CubicLerp(NoiseHelper.ValueCoord(Seed, x0, y1), NoiseHelper.ValueCoord(Seed, x1, y1), NoiseHelper.ValueCoord(Seed, x2, y1), NoiseHelper.ValueCoord(Seed, x3, y1),
                      xs),
            MathHelper.CubicLerp(NoiseHelper.ValueCoord(Seed, x0, y2), NoiseHelper.ValueCoord(Seed, x1, y2), NoiseHelper.ValueCoord(Seed, x2, y2), NoiseHelper.ValueCoord(Seed, x3, y2),
                      xs),
            MathHelper.CubicLerp(NoiseHelper.ValueCoord(Seed, x0, y3), NoiseHelper.ValueCoord(Seed, x1, y3), NoiseHelper.ValueCoord(Seed, x2, y3), NoiseHelper.ValueCoord(Seed, x3, y3),
                      xs),
            ys) * (1.0f / (1.5f * 1.5f));
    }

    public void TransformCoordinate(ref float x, ref float y)
    {
    }
}