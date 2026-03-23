namespace FantaniaLib;

public class CellularNoise2DParameters : NoiseParameters
{
    private float _jitterModifier = 1.0f;
    [EditableField(EditParameter = "-1:1:0.05")]
    public float JitterModifier
    {
        get { return _jitterModifier; }
        set
        {
            if (_jitterModifier != value)
            {
                _jitterModifier = value;
                OnPropertyChanged(nameof(JitterModifier));
            }
        }
    }

    private NoiseHelper.CellularDistanceFunctions _disFunc = NoiseHelper.CellularDistanceFunctions.Euclidean;
    [EditableField]
    public NoiseHelper.CellularDistanceFunctions DistanceFunction
    {
        get { return _disFunc; }
        set
        {
            if (_disFunc != value)
            {
                _disFunc = value;
                OnPropertyChanged(nameof(DistanceFunction));
            }
        }
    }

    private NoiseHelper.CellularReturnTypes _retType = NoiseHelper.CellularReturnTypes.Distance;
    [EditableField]
    public NoiseHelper.CellularReturnTypes ReturnType
    {
        get { return _retType; }
        set
        {
            if (_retType != value)
            {
                _retType = value;
                OnPropertyChanged(nameof(ReturnType));
            }
        }
    }
}

public class CellularNoise2D : INoise2D
{
    public int Seed { get; set; } = 0;
    public CellularNoise2DParameters Arguments { get; private set; }

    public CellularNoise2D(CellularNoise2DParameters args)
    {
        Arguments = args;
    }

    public void TransformCoordinate(ref float x, ref float y)
    {
    }

    public float Noise(float x, float y, int repeat)
    {
        if (repeat > 0)
        {
            x = ((x % repeat) + repeat) % repeat;
            y = ((y % repeat) + repeat) % repeat;
        }
        int xr = MathHelper.FloorToInt(x);
        int yr = MathHelper.FloorToInt(y);
        float distance0 = float.MaxValue;
        float distance1 = float.MaxValue;
        int closestHash = 0;
        float cellularJitter = 0.43701595f * (float)Arguments.JitterModifier;
        const int RANGE = 2;
        for (int xi = xr - RANGE; xi <= xr + RANGE; xi++)
        {
            int px = xi;
            if (repeat > 0)
                px = ((px % repeat) + repeat) % repeat;
            int xPrimed = px * NoiseHelper.PrimeX;
            for (int yi = yr - RANGE; yi <= yr + RANGE; yi++)
            {
                int py = yi;
                if (repeat > 0)
                    py = ((py % repeat) + repeat) % repeat;
                int yPrimed = py * NoiseHelper.PrimeY;
                int hash = NoiseHelper.Hash(Seed, xPrimed, yPrimed);
                int idx = hash & (255 << 1);
                float fx = px + NoiseHelper.RandomVectors2D[idx] * cellularJitter;
                float fy = py + NoiseHelper.RandomVectors2D[idx | 1] * cellularJitter;
                float dx = fx - x;
                float dy = fy - y;
                if (repeat > 0)
                {
                    if (dx > repeat * 0.5f)
                        dx -= repeat;
                    else if (dx < -repeat * 0.5f)
                        dx += repeat;
                    if (dy > repeat * 0.5f)
                        dy -= repeat;
                    else if (dy < -repeat * 0.5f)
                        dy += repeat;
                }
                float newDistance;
                switch (Arguments.DistanceFunction)
                {
                    case NoiseHelper.CellularDistanceFunctions.Manhattan:
                        newDistance = MathF.Abs(dx) + MathF.Abs(dy);
                        break;
                    case NoiseHelper.CellularDistanceFunctions.Hybrid:
                        newDistance = MathF.Abs(dx) + MathF.Abs(dy) + (dx * dx + dy * dy);
                        break;
                    case NoiseHelper.CellularDistanceFunctions.Euclidean:
                    case NoiseHelper.CellularDistanceFunctions.EuclideanSquared:
                    default:
                        newDistance = dx * dx + dy * dy;
                        break;
                }

                distance1 = MathHelper.Clamp(newDistance, distance0, distance1);
                if (newDistance < distance0)
                {
                    distance0 = newDistance;
                    closestHash = hash;
                }
            }
        }

        if (Arguments.DistanceFunction == NoiseHelper.CellularDistanceFunctions.Euclidean && Arguments.ReturnType >= NoiseHelper.CellularReturnTypes.Distance)
        {
            distance0 = MathF.Sqrt(distance0);
            if (Arguments.ReturnType >= NoiseHelper.CellularReturnTypes.Distance2)
            {
                distance1 = MathF.Sqrt(distance1);
            }
        }

        switch (Arguments.ReturnType)
        {
            case NoiseHelper.CellularReturnTypes.CellValue:
                return closestHash * (1.0f / 2147483648.0f);
            case NoiseHelper.CellularReturnTypes.Distance:
                return distance0 - 1.0f;
            case NoiseHelper.CellularReturnTypes.Distance2:
                return distance1 - 1.0f;
            case NoiseHelper.CellularReturnTypes.Distance2Add:
                return (distance1 + distance0) * 0.5f - 1.0f;
            case NoiseHelper.CellularReturnTypes.Distance2Sub:
                return distance1 - distance0 - 1.0f;
            case NoiseHelper.CellularReturnTypes.Distance2Mul:
                return distance1 * distance0 * 0.5f - 1.0f;
            case NoiseHelper.CellularReturnTypes.Distance2Div:
                return distance0 / distance1 - 1.0f;
        }
        return 0.0f;
    }
}
