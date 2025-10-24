using System;

namespace Fantania;

public enum NoiseTypes
{
    Cellular,
    Perlin,
    Simplex,
    Value,
}

public interface INoiseParameters : IEquatable<INoiseParameters>
{
    void CopyFrom(INoiseParameters other);
}

[DisableEditing("NoiseType")]
public struct Noise2DLite : IEquatable<Noise2DLite>
{
    NoiseTypes _type = NoiseTypes.Cellular;
    [EditEnum]
    public NoiseTypes NoiseType
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnNoiseTypeChange(_type);
            }
        }
    }
    int _seed = 0;
    [EditInteger(ControlType = typeof(RandomSeedControl))]
    public int Seed
    {
        get => _seed;
        set
        {
            if (_seed != value)
            {
                _seed = value;
                _noise.Seed = _seed;
            }
        }
    }

    [EditInteger(1, 4, ControlType = typeof(UpDownIntegerControl))]
    public int Octaves { get; set; }

    [EditDecimal(0.02, 1.0, ControlType = typeof(SliderDecimalControl))]
    public double Frequency { get; set; }

    [EditBoolean]
    public bool Inverted { get; set; }

    [EditBoolean]
    public bool Repeat { get; set; }

    public INoiseParameters NoiseArguments { get; set; }

    public Noise2DLite()
    {
        _argslist[0] = new CellularNoiseParameters();
        _argslist[1] = new PerlinNoiseParameters();
        _argslist[2] = new SimplexNoiseParameters();
        _argslist[3] = new ValueNoiseParameters();
        _type = NoiseTypes.Cellular;
        OnNoiseTypeChange(_type);
        Seed = 0;
        Octaves = 1;
        Frequency = 0.05;
        Inverted = Repeat = false;
    }

    public float Get(float x, float y, int size)
    {
        int repeat = Repeat ? MathHelper.RoundToInt(size * Frequency) : 0;
        float realFreq = repeat == 0 ? (float)Frequency : (float)repeat / size;
        x *= realFreq;
        y *= realFreq;
        _noise.TransformCoordinate(ref x, ref y);
        float total = 0.0f;
        float freq = 1.0f;
        float amplitude = 1.0f;
        float max = 0.0f;
        for (int i = 0; i < Octaves; i++)
        {
            float noise = _noise.Noise(x * freq, y * freq, (int)(repeat * freq));
            if (Inverted)
            {
                noise = -noise;
            }
            total += noise;
            max += amplitude;
            freq *= 2.0f;
        }
        float ret = total / max;
        return ret;
    }

    public Noise2DLite Clone()
    {
        var ret = new Noise2DLite();
        for (int i = 0; i < _argslist.Length; i++)
        {
            ret._argslist[i].CopyFrom(_argslist[i]);
        }
        ret.NoiseType = NoiseType;
        ret.Octaves = Octaves;
        ret.Seed = Seed;
        ret.Frequency = Frequency;
        ret.Inverted = Inverted;
        ret.Repeat = Repeat;
        return ret;
    }

    public static bool operator ==(Noise2DLite lhs, Noise2DLite rhs)
    {
        return lhs.NoiseType == rhs.NoiseType && lhs.Seed == rhs.Seed && lhs.Octaves == rhs.Octaves && lhs.Frequency == rhs.Frequency && lhs.Inverted == rhs.Inverted && lhs.Repeat == rhs.Repeat && lhs.NoiseArguments.Equals(rhs.NoiseArguments);
    }

    public static bool operator !=(Noise2DLite lhs, Noise2DLite rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(Noise2DLite other)
    {
        return this == other;
    }

    public override bool Equals(object obj)
    {
        return obj is Noise2DLite && Equals((Noise2DLite)obj);
    }

    public override int GetHashCode()
    {
        int hash = (int)NoiseType;
        hash = (hash * 397) ^ Seed;
        hash = (hash * 397) ^ Octaves;
        hash = (hash * 397) ^ Frequency.GetHashCode();
        hash = (hash * 397) ^ Inverted.GetHashCode();
        hash = (hash * 397) ^ Repeat.GetHashCode();
        hash = (hash * 397) ^ NoiseArguments.GetHashCode();
        return hash;
    }

    void OnNoiseTypeChange(NoiseTypes type)
    {
        NoiseArguments = _argslist[(int)type];
        switch (type)
        {
            case NoiseTypes.Perlin:
                _noise = new PerlinNoise2D((PerlinNoiseParameters)NoiseArguments)
                {
                    Seed = Seed,
                };
                break;
            case NoiseTypes.Cellular:
                _noise = new CellularNoise2D((CellularNoiseParameters)NoiseArguments)
                {
                    Seed = Seed,
                };
                break;
            case NoiseTypes.Simplex:
                _noise = new SimplexNoise2D((SimplexNoiseParameters)NoiseArguments)
                {
                    Seed = Seed,
                };
                break;
            case NoiseTypes.Value:
                _noise = new ValueNoise2D((ValueNoiseParameters)NoiseArguments)
                {
                    Seed = Seed,
                };
                break;
            default:
                break;
        }
    }

    INoise2D _noise;
    INoiseParameters[] _argslist = new INoiseParameters[Enum.GetValues<NoiseTypes>().Length];
}