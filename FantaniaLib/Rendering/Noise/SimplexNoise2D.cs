namespace FantaniaLib;

public class SimplexNoise2DParameters : NoiseParameters
{
    private NoiseHelper.SimplexAlgorithms _algorithm = NoiseHelper.SimplexAlgorithms.OrdinarySimplex;
    [EditableField(TooltipKey = "TT_SimplexAlgorithm")]
    public NoiseHelper.SimplexAlgorithms Algorithm
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

public class SimplexNoise2D : INoise2D
{
    public int Seed { get; set; }
    public SimplexNoise2DParameters Arguments { get; private set; }

    public SimplexNoise2D(SimplexNoise2DParameters args)
    {
        Arguments = args;
    }

    public void TransformCoordinate(ref float x, ref float y)
    {
        float s = (x + y) * F2;
        x += s;
        y += s;
    }

    public float Noise(float x, float y, int repeat)
    {
        switch (Arguments.Algorithm)
        {
            case NoiseHelper.SimplexAlgorithms.OpenSimplex2S:
                return OpenSimplex2S(x, y, repeat);
            case NoiseHelper.SimplexAlgorithms.OrdinarySimplex:
            default:
                return OrdinarySimplex(x, y, repeat);
        }
    }

    float OrdinarySimplex(float x, float y, int repeat)
    {
        int i0 = MathHelper.FloorToInt(x);
        int j0 = MathHelper.FloorToInt(y);
        int i1 = i0 + 1;
        int j1 = j0 + 1;
        if (repeat > 0)
        {
            i0 = ((i0 % repeat) + repeat) % repeat;
            j0 = ((j0 % repeat) + repeat) % repeat;
            i1 = ((i1 % repeat) + repeat) % repeat;
            j1 = ((j1 % repeat) + repeat) % repeat;
        }
        float xi = x - MathHelper.FloorToInt(x);
        float yi = y - MathHelper.FloorToInt(y);
        float t = (xi + yi) * G2;
        float x0 = xi - t;
        float y0 = yi - t;
        int ip0 = i0 * NoiseHelper.PrimeX;
        int jp0 = j0 * NoiseHelper.PrimeY;
        int ip1 = i1 * NoiseHelper.PrimeX;
        int jp1 = j1 * NoiseHelper.PrimeY;
        float n0, n1, n2;
        float a = 0.5f - x0 * x0 - y0 * y0;
        if (a <= 0)
            n0 = 0;
        else
            n0 = (a * a) * (a * a) * NoiseHelper.GradientCoord(Seed, ip0, jp0, x0, y0);
        float c = 2.0f * (1.0f - 2.0f * G2) * (1.0f / G2 - 2.0f) * t + ((-2.0f * (1.0f - 2.0f * G2) * (1.0f - 2.0f * G2)) + a);
        if (c <= 0)
            n2 = 0;
        else
        {
            float x2 = x0 + (2.0f * G2 - 1.0f);
            float y2 = y0 + (2.0f * G2 - 1.0f);
            n2 = (c * c) * (c * c) * NoiseHelper.GradientCoord(Seed, ip1, jp1, x2, y2);
        }
        if (y0 > x0)
        {
            float x1 = x0 + G2;
            float y1 = y0 + (G2 - 1.0f);
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b <= 0) n1 = 0;
            else
            {
                n1 = (b * b) * (b * b) * NoiseHelper.GradientCoord(Seed, ip0, jp1, x1, y1);
            }
        }
        else
        {
            float x1 = x0 + (G2 - 1.0f);
            float y1 = y0 + G2;
            float b = 0.5f - x1 * x1 - y1 * y1;
            if (b <= 0)
                n1 = 0;
            else
            {
                n1 = (b * b) * (b * b) * NoiseHelper.GradientCoord(Seed, ip1, jp0, x1, y1);
            }
        }
        return (n0 + n1 + n2) * 99.83685446303647f;
    }

    float OpenSimplex2S(float x, float y, int repeat)
    {
        int i0 = MathHelper.FloorToInt(x);
        int j0 = MathHelper.FloorToInt(y);
        int i1 = i0 + 1;
        int j1 = j0 + 1;
        int i2 = i0 + 2;
        int j2 = j0 + 2;
        int im1 = i0 - 1;
        int jm1 = j0 - 1;
        if (repeat > 0)
        {
            i0 = ((i0 % repeat) + repeat) % repeat;
            j0 = ((j0 % repeat) + repeat) % repeat;
            i1 = ((i1 % repeat) + repeat) % repeat;
            j1 = ((j1 % repeat) + repeat) % repeat;
            i2 = ((i2 % repeat) + repeat) % repeat;
            j2 = ((j2 % repeat) + repeat) % repeat;
            im1 = ((im1 % repeat) + repeat) % repeat;
            jm1 = ((jm1 % repeat) + repeat) % repeat;
        }
        float xi = x - MathHelper.FloorToInt(x);
        float yi = y - MathHelper.FloorToInt(y);
        float t = (xi + yi) * G2;
        float x0 = xi - t;
        float y0 = yi - t;
        int ip0 = i0 * NoiseHelper.PrimeX;
        int jp0 = j0 * NoiseHelper.PrimeY;
        int ip1 = i1 * NoiseHelper.PrimeX;
        int jp1 = j1 * NoiseHelper.PrimeY;
        int ip2 = i2 * NoiseHelper.PrimeX;
        int jp2 = j2 * NoiseHelper.PrimeY;
        int ipm1 = im1 * NoiseHelper.PrimeX;
        int jpm1 = jm1 * NoiseHelper.PrimeY;
        float a0 = (2.0f / 3.0f) - x0 * x0 - y0 * y0;
        float value = (a0 * a0) * (a0 * a0) * NoiseHelper.GradientCoord(Seed, ip0, jp0, x0, y0);
        float a1 = (2.0f * (1.0f - 2.0f * G2) * (1.0f / G2 - 2.0f)) * t + ((float)(-2.0f * (1.0f - 2.0f * G2) * (1.0f - 2.0f * G2)) + a0);
        float x1 = x0 - (1.0f - 2.0f * G2);
        float y1 = y0 - (1.0f - 2.0f * G2);
        value += (a1 * a1) * (a1 * a1) * NoiseHelper.GradientCoord(Seed, ip1, jp1, x1, y1);
        float xmyi = xi - yi;
        if (t > G2)
        {
            if (xi + xmyi > 1.0f)
            {
                float x2 = x0 + (3.0f * G2 - 2.0f);
                float y2 = y0 + (3.0f * G2 - 1.0f);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0.0f)
                {
                    value += (a2 * a2) * (a2 * a2) * NoiseHelper.GradientCoord(Seed, ip2, jp1, x2, y2);
                }
            }
            else
            {
                float x2 = x0 + G2;
                float y2 = y0 + (G2 - 1.0f);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * NoiseHelper.GradientCoord(Seed, ip0, jp1, x2, y2);
                }
            }
            if (yi - xmyi > 1.0f)
            {
                float x3 = x0 + (3.0f * G2 - 1.0f);
                float y3 = y0 + (3.0f * G2 - 2.0f);
                float a3 = (2.0f / 3.0f) - x3 * x3 - y3 * y3;
                if (a3 > 0)
                {
                    value += (a3 * a3) * (a3 * a3) * NoiseHelper.GradientCoord(Seed, ip1, jp2, x3, y3);
                }
            }
            else
            {
                float x3 = x0 + (G2 - 1.0f);
                float y3 = y0 + G2;
                float a3 = (2.0f / 3.0f) - x3 * x3 - y3 * y3;
                if (a3 > 0)
                {
                    value += (a3 * a3) * (a3 * a3) * NoiseHelper.GradientCoord(Seed, ip1, jp0, x3, y3);
                }
            }
        }
        else
        {
            if (xi + xmyi < 0.0f)
            {
                float x2 = x0 + (1.0f - G2);
                float y2 = y0 - G2;
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * NoiseHelper.GradientCoord(Seed, ipm1, jp0, x2, y2);
                }
            }
            else
            {
                float x2 = x0 + (G2 - 1.0f);
                float y2 = y0 + G2;
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * NoiseHelper.GradientCoord(Seed, ip1, jp0, x2, y2);
                }
            }
            if (yi < xmyi)
            {
                float x2 = x0 - G2;
                float y2 = y0 - (G2 - 1.0f);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * NoiseHelper.GradientCoord(Seed, ip0, jpm1, x2, y2);
                }
            }
            else
            {
                float x2 = x0 + G2;
                float y2 = y0 + (G2 - 1.0f);
                float a2 = (2.0f / 3.0f) - x2 * x2 - y2 * y2;
                if (a2 > 0)
                {
                    value += (a2 * a2) * (a2 * a2) * NoiseHelper.GradientCoord(Seed, ip0, jp1, x2, y2);
                }
            }
        }
        return value * 18.24196194486065f;
    }

    const float SQRT3 = 1.7320508075688772935274463415059f;
    const float G2 = (3.0f - SQRT3) / 6.0f;
    const float F2 = 0.5f * (SQRT3 - 1.0f);
}