using System.Reflection;

namespace FantaniaLib;

public enum NoiseTypes
{
    Perlin,
    Cellular,
    Simplex,
    Value,
}

public enum NoiseCompositors
{
    None,
    RidgedMultiFractal,
}

public class NoiseParameters : FantaniaObject
{
    protected NoiseParameters()
    { }

    public void CopyTo(NoiseParameters other)
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.GetCustomAttribute<EditableFieldAttribute>() == null)
                continue;
            if (prop.CanRead && prop.CanWrite)
            {
                var value = prop.GetValue(this);
                prop.SetValue(other, value);
            }
        }
    }

    public override string OnCopy(IWorkspace workspace)
    {
        throw new NotImplementedException();
    }

    public override void OnPaste(IWorkspace workspace, string serializedData)
    {
        throw new NotImplementedException();
    }
}

public class NoiseCompositorParameters : FantaniaObject
{
    public static readonly NoiseCompositorParameters Empty = new NoiseCompositorParameters();

    protected NoiseCompositorParameters()
    { }

    public void CopyTo(NoiseCompositorParameters other)
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
            if (prop.GetCustomAttribute<EditableFieldAttribute>() == null)
                continue;
            if (prop.CanRead && prop.CanWrite)
            {
                var value = prop.GetValue(this);
                prop.SetValue(other, value);
            }
        }
    }

    public override string OnCopy(IWorkspace workspace)
    {
        throw new NotImplementedException();
    }

    public override void OnPaste(IWorkspace workspace, string serializedData)
    {
        throw new NotImplementedException();
    }
}

public class Noise2DLite : FantaniaObject
{
    private NoiseTypes _noiseType = NoiseTypes.Perlin;
    [EditableField(TooltipKey = "TT_NoiseType")]
    public NoiseTypes NoiseType
    {
        get { return _noiseType; }
        set
        {
            if (_noiseType != value)
            {
                _noiseType = value;
                OnPropertyChanged(nameof(NoiseType));
                NoiseArguments = _cachedArgs[(int)_noiseType];
            }
        }
    }

    private int _seed = 0;
    [EditableField(EditControlType = typeof(RandomSeedBox), TooltipKey = "TT_RandomSeed")]
    public int Seed
    {
        get { return _seed; }
        set
        {
            if (_seed != value)
            {
                _seed = value;
                OnPropertyChanged(nameof(Seed));
                for (int i = 0; i < _cachedNoises.Length; i++)
                {
                    _cachedNoises[i].Seed = _seed;
                }
            }
        }
    }

    private int _octaves = 1;
    [EditableField(EditParameter = "1:8:1", TooltipKey = "TT_NoiseOctaves")]
    public int Octaves
    {
        get { return _octaves; }
        set
        {
            if (_octaves != value)
            {
                _octaves = value;
                OnPropertyChanged(nameof(Octaves));
            }
        }
    }

    private float _freq = 0.05f;
    [EditableField(EditParameter = "0.01:1.0:0.001", TooltipKey = "TT_NoiseFrequency")]
    public float Frequency
    {
        get { return _freq; }
        set
        {
            if (_freq != value)
            {
                _freq = value;
                OnPropertyChanged(nameof(Frequency));
            }
        }
    }

    private float _persistence = 0.5f;
    [EditableField(EditParameter = "0.1:1.0:0.01", TooltipKey = "TT_NoisePersistence")]
    public float Persistence
    {
        get { return _persistence; }
        set
        {
            if (_persistence != value)
            {
                _persistence = value;
                OnPropertyChanged(nameof(Persistence));
            }
        }
    }

    private float _lacunarity = 2.0f;
    [EditableField(EditParameter = "1.0:4.0:0.01", TooltipKey = "TT_NoiseLacunarity")]
    public float Lacunarity
    {
        get { return _lacunarity; }
        set
        {
            if (_lacunarity != value)
            {
                _lacunarity = value;
                OnPropertyChanged(nameof(Lacunarity));
            }
        }
    }

    private bool _inverted = false;
    [EditableField(TooltipKey = "TT_NoiseInverted")]
    public bool Inverted
    {
        get { return _inverted; }
        set
        {
            if (_inverted != value)
            {
                _inverted = value;
                OnPropertyChanged(nameof(Inverted));
            }
        }
    }

    private bool _repeat = false;
    [EditableField(TooltipKey = "TT_NoiseRepeat")]
    public bool Repeat
    {
        get { return _repeat; }
        set
        {
            if (_repeat != value)
            {
                _repeat = value;
                OnPropertyChanged(nameof(Repeat));
            }
        }
    }

    private Vector2Int _size = new Vector2Int(256, 256);
    [EditableField(EditParameter = "32:2048", TooltipKey = "TT_NoiseSize")]
    public Vector2Int Size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
    }

    private NoiseCompositors _compositor = NoiseCompositors.None;
    [EditableField(TooltipKey = "TT_NoiseCompositor")]
    public NoiseCompositors Compositor
    {
        get { return _compositor; }
        set
        {
            if (_compositor != value)
            {
                _compositor = value;
                OnPropertyChanged(nameof(Compositor));
                CompositorArguments = _cachedCompositorArgs[(int)_compositor];
            }
        }
    }

    NoiseParameters _noiseArgs;
    public NoiseParameters NoiseArguments
    {
        get { return _noiseArgs; }
        set
        {
            if (_noiseArgs != value)
            {
                _noiseArgs = value;
                OnPropertyChanged(nameof(NoiseArguments));
            }
        }
    }

    NoiseCompositorParameters _compositorArgs;
    public NoiseCompositorParameters CompositorArguments
    {
        get { return _compositorArgs; }
        set
        {
            if (_compositorArgs != value)
            {
                _compositorArgs = value;
                OnPropertyChanged(nameof(CompositorArguments));
            }
        }
    }

    public INoise2D CurrentNoise => _cachedNoises[(int)_noiseType];
    public INoise2DCompositor CurrentCompositor => _cachedCompositors[(int)_compositor];

    public Noise2DLite()
    {
        _cachedArgs[(int)NoiseTypes.Perlin] = new PerlinNoise2DParameters();
        _cachedArgs[(int)NoiseTypes.Cellular] = new CellularNoise2DParameters();
        _cachedArgs[(int)NoiseTypes.Simplex] = new SimplexNoise2DParameters();
        _cachedArgs[(int)NoiseTypes.Value] = new ValueNoise2DParameters();
        _noiseArgs = _cachedArgs[(int)_noiseType];
        _cachedCompositorArgs[(int)NoiseCompositors.None] = NoiseCompositorParameters.Empty;
        _cachedCompositorArgs[(int)NoiseCompositors.RidgedMultiFractal] = new RidgedMultiFractalCompositorParameters();
        _compositorArgs = _cachedCompositorArgs[(int)_compositor];
        _cachedNoises[(int)NoiseTypes.Perlin] = new PerlinNoise2D((PerlinNoise2DParameters)_cachedArgs[(int)NoiseTypes.Perlin]);
        _cachedNoises[(int)NoiseTypes.Cellular] = new CellularNoise2D((CellularNoise2DParameters)_cachedArgs[(int)NoiseTypes.Cellular]);
        _cachedNoises[(int)NoiseTypes.Simplex] = new SimplexNoise2D((SimplexNoise2DParameters)_cachedArgs[(int)NoiseTypes.Simplex]);
        _cachedNoises[(int)NoiseTypes.Value] = new ValueNoise2D((ValueNoise2DParameters)_cachedArgs[(int)NoiseTypes.Value]);
        _cachedCompositors[(int)NoiseCompositors.None] = new EmptyNoiseCompositor();
        _cachedCompositors[(int)NoiseCompositors.RidgedMultiFractal] = new RidgedMultiFractalCompositor((RidgedMultiFractalCompositorParameters)_cachedCompositorArgs[(int)NoiseCompositors.RidgedMultiFractal]);
    }

    public float Get(float x, float y, int size)
    {
        int repeat = Repeat ? MathHelper.RoundToInt(size * Frequency) : 0;
        float realFreq = repeat == 0 ? (float)Frequency : (float)repeat / size;
        x *= realFreq;
        y *= realFreq;
        CurrentNoise.TransformCoordinate(ref x, ref y);
        float total = 0.0f;
        float freq = 1.0f;
        float amplitude = 1.0f;
        float max = 0.0f;
        for (int i = 0; i < Octaves; i++)
        {
            // float noise = CurrentNoise.Noise(x * freq, y * freq, (int)(repeat * freq));
            float noise = CurrentCompositor.Composite(CurrentNoise, x * freq, y * freq, (int)(repeat * freq));
            if (Inverted)
            {
                noise = -noise;
            }
            total += noise * amplitude;
            max += amplitude;
            amplitude *= Persistence;
            freq *= Lacunarity;
        }
        float ret = total / max;
        return ret;
    }

    public NoiseParameters NoiseParameterAt(NoiseTypes type)
    {
        return _cachedArgs[(int)type];
    }

    public NoiseCompositorParameters CompositorParameterAt(NoiseCompositors type)
    {
        return _cachedCompositorArgs[(int)type];
    }

    public Noise2DLite Clone()
    {
        Noise2DLite clone = new Noise2DLite();
        clone.NoiseType = NoiseType;
        clone.Seed = Seed;
        clone.Octaves = Octaves;
        clone.Frequency = Frequency;
        clone.Persistence = Persistence;
        clone.Lacunarity = Lacunarity;
        clone.Inverted = Inverted;
        clone.Repeat = Repeat;
        clone.Size = Size;
        clone.Compositor = Compositor;
        for (int i = 0; i < _cachedArgs.Length; i++)
        {
            _cachedArgs[i].CopyTo(clone._cachedArgs[i]);
            clone._cachedNoises[i].Seed = clone.Seed;
        }
        for (int i = 0; i < _cachedCompositorArgs.Length; i++)
        {
            _cachedCompositorArgs[i].CopyTo(clone._cachedCompositorArgs[i]);
        }
        return clone;
    }

    public override string OnCopy(IWorkspace workspace)
    {
        throw new NotImplementedException();
    }

    public override void OnPaste(IWorkspace workspace, string serializedData)
    {
    }

    readonly NoiseParameters[] _cachedArgs = new NoiseParameters[Enum.GetValues<NoiseTypes>().Length];
    readonly NoiseCompositorParameters[] _cachedCompositorArgs = new NoiseCompositorParameters[Enum.GetValues<NoiseCompositors>().Length];
    readonly INoise2D[] _cachedNoises = new INoise2D[Enum.GetValues<NoiseTypes>().Length];
    readonly INoise2DCompositor[] _cachedCompositors = new INoise2DCompositor[Enum.GetValues<NoiseCompositors>().Length];
}