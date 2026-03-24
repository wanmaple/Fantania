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
        // Skew is applied inside Noise() so that repeat wrapping
        // operates on un-skewed integer coordinates, enabling correct tiling.
    }

    public float Noise(float x, float y, int repeat)
    {
        if (repeat > 0)
        {
            return TileableSimplex4D(x, y, repeat);
        }
        switch (Arguments.Algorithm)
        {
            case NoiseHelper.SimplexAlgorithms.OpenSimplex2S:
                return OpenSimplex2S(x, y, repeat);
            case NoiseHelper.SimplexAlgorithms.OrdinarySimplex:
            default:
                return OrdinarySimplex(x, y, repeat);
        }
    }

    float TileableSimplex4D(float x, float y, int repeat)
    {
        // Map 2D coordinates onto a 4D torus. This guarantees strict periodicity in x/y.
        float period = MathF.Max(1.0f, repeat);
        float angleX = x * TWO_PI / period;
        float angleY = y * TWO_PI / period;
        float radius = period / TWO_PI;
        float nx = MathF.Cos(angleX) * radius;
        float ny = MathF.Sin(angleX) * radius;
        float nz = MathF.Cos(angleY) * radius;
        float nw = MathF.Sin(angleY) * radius;
        return Simplex4D(nx, ny, nz, nw);
    }

    float Simplex4D(float x, float y, float z, float w)
    {
        float s = (x + y + z + w) * F4;
        int i = MathHelper.FloorToInt(x + s);
        int j = MathHelper.FloorToInt(y + s);
        int k = MathHelper.FloorToInt(z + s);
        int l = MathHelper.FloorToInt(w + s);
        float t = (i + j + k + l) * G4;
        float x0 = x - (i - t);
        float y0 = y - (j - t);
        float z0 = z - (k - t);
        float w0 = w - (l - t);
        int rankx = 0;
        int ranky = 0;
        int rankz = 0;
        int rankw = 0;
        if (x0 > y0) rankx++; else ranky++;
        if (x0 > z0) rankx++; else rankz++;
        if (x0 > w0) rankx++; else rankw++;
        if (y0 > z0) ranky++; else rankz++;
        if (y0 > w0) ranky++; else rankw++;
        if (z0 > w0) rankz++; else rankw++;
        int i1 = rankx >= 3 ? 1 : 0;
        int j1 = ranky >= 3 ? 1 : 0;
        int k1 = rankz >= 3 ? 1 : 0;
        int l1 = rankw >= 3 ? 1 : 0;
        int i2 = rankx >= 2 ? 1 : 0;
        int j2 = ranky >= 2 ? 1 : 0;
        int k2 = rankz >= 2 ? 1 : 0;
        int l2 = rankw >= 2 ? 1 : 0;
        int i3 = rankx >= 1 ? 1 : 0;
        int j3 = ranky >= 1 ? 1 : 0;
        int k3 = rankz >= 1 ? 1 : 0;
        int l3 = rankw >= 1 ? 1 : 0;
        float x1 = x0 - i1 + G4;
        float y1 = y0 - j1 + G4;
        float z1 = z0 - k1 + G4;
        float w1 = w0 - l1 + G4;
        float x2 = x0 - i2 + 2.0f * G4;
        float y2 = y0 - j2 + 2.0f * G4;
        float z2 = z0 - k2 + 2.0f * G4;
        float w2 = w0 - l2 + 2.0f * G4;
        float x3 = x0 - i3 + 3.0f * G4;
        float y3 = y0 - j3 + 3.0f * G4;
        float z3 = z0 - k3 + 3.0f * G4;
        float w3 = w0 - l3 + 3.0f * G4;
        float x4 = x0 - 1.0f + 4.0f * G4;
        float y4 = y0 - 1.0f + 4.0f * G4;
        float z4 = z0 - 1.0f + 4.0f * G4;
        float w4 = w0 - 1.0f + 4.0f * G4;
        float n0 = Corner4(i, j, k, l, x0, y0, z0, w0);
        float n1 = Corner4(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1);
        float n2 = Corner4(i + i2, j + j2, k + k2, l + l2, x2, y2, z2, w2);
        float n3 = Corner4(i + i3, j + j3, k + k3, l + l3, x3, y3, z3, w3);
        float n4 = Corner4(i + 1, j + 1, k + 1, l + 1, x4, y4, z4, w4);
        return 27.0f * (n0 + n1 + n2 + n3 + n4);
    }

    float Corner4(int i, int j, int k, int l, float x, float y, float z, float w)
    {
        float t = 0.6f - x * x - y * y - z * z - w * w;
        if (t <= 0.0f)
        {
            return 0.0f;
        }
        t *= t;
        return t * t * GradientCoord4(i, j, k, l, x, y, z, w);
    }

    float GradientCoord4(int i, int j, int k, int l, float x, float y, float z, float w)
    {
        int hash = Hash4(i, j, k, l);
        int gi = hash & 31;
        int index = gi * 4;
        return x * Gradients4D[index]
            + y * Gradients4D[index + 1]
            + z * Gradients4D[index + 2]
            + w * Gradients4D[index + 3];
    }

    int Hash4(int i, int j, int k, int l)
    {
        int hash = Seed;
        hash ^= i * NoiseHelper.PrimeX;
        hash ^= j * NoiseHelper.PrimeY;
        hash ^= k * PrimeZ;
        hash ^= l * PrimeW;
        hash *= 0x27d4eb2d;
        hash ^= hash >> 15;
        return hash;
    }

    float OrdinarySimplex(float x, float y, int repeat)
    {
        float s = (x + y) * F2;
        x += s;
        y += s;
        int i0 = MathHelper.FloorToInt(x);
        int j0 = MathHelper.FloorToInt(y);
        int i1 = i0 + 1;
        int j1 = j0 + 1;
        float xi = x - i0;
        float yi = y - j0;
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
        float s = (x + y) * F2;
        x += s;
        y += s;
        int i0 = MathHelper.FloorToInt(x);
        int j0 = MathHelper.FloorToInt(y);
        int i1 = i0 + 1;
        int j1 = j0 + 1;
        int i2 = i0 + 2;
        int j2 = j0 + 2;
        int im1 = i0 - 1;
        int jm1 = j0 - 1;
        float xi = x - i0;
        float yi = y - j0;
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
    const float SQRT5 = 2.2360679774997896964091736687313f;
    const float F4 = (SQRT5 - 1.0f) / 4.0f;
    const float G4 = (5.0f - SQRT5) / 20.0f;
    const float TWO_PI = MathF.PI * 2.0f;
    const int PrimeZ = 1720413743;
    const int PrimeW = 19990303;

    static readonly float[] Gradients4D =
    {
        0, 1, 1, 1, 0, 1, 1, -1, 0, 1, -1, 1, 0, 1, -1, -1,
        0, -1, 1, 1, 0, -1, 1, -1, 0, -1, -1, 1, 0, -1, -1, -1,
        1, 0, 1, 1, 1, 0, 1, -1, 1, 0, -1, 1, 1, 0, -1, -1,
        -1, 0, 1, 1, -1, 0, 1, -1, -1, 0, -1, 1, -1, 0, -1, -1,
        1, 1, 0, 1, 1, 1, 0, -1, 1, -1, 0, 1, 1, -1, 0, -1,
        -1, 1, 0, 1, -1, 1, 0, -1, -1, -1, 0, 1, -1, -1, 0, -1,
        1, 1, 1, 0, 1, 1, -1, 0, 1, -1, 1, 0, 1, -1, -1, 0,
        -1, 1, 1, 0, -1, 1, -1, 0, -1, -1, 1, 0, -1, -1, -1, 0,
    };
}