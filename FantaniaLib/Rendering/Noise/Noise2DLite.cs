namespace FantaniaLib;

public enum NoiseTypes
{
    Perlin,
    Cellular,
    Simplex,
    Value,
}

public class NoiseParameters : FantaniaObject
{
    protected NoiseParameters()
    {}

    public void CopyTo(NoiseParameters other)
    {
        var props = GetType().GetProperties();
        foreach (var prop in props)
        {
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
    }
}

public class Noise2DLite : FantaniaObject
{
    private NoiseTypes _noiseType = NoiseTypes.Perlin;
    [EditableField]
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
    [EditableField(EditControlType = typeof(RandomSeedBox))]
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
    [EditableField(EditParameter = "1:8:1")]
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
    [EditableField(EditParameter = "0.01:1.0:0.01")]
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

    private bool _inverted = false;
    [EditableField]
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
    [EditableField]
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
    [EditableField(EditParameter = "32:2048")]
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

    public INoise2D CurrentNoise => _cachedNoises[(int)_noiseType];

    public Noise2DLite()
    {
        _cachedArgs[(int)NoiseTypes.Perlin] = new PerlinNoise2DParameters();
        _cachedArgs[(int)NoiseTypes.Cellular] = new CellularNoise2DParameters();
        _cachedArgs[(int)NoiseTypes.Simplex] = new SimplexNoise2DParameters();
        _cachedArgs[(int)NoiseTypes.Value] = new ValueNoise2DParameters();
        _noiseArgs = _cachedArgs[(int)_noiseType];
        _cachedNoises[(int)NoiseTypes.Perlin] = new PerlinNoise2D((PerlinNoise2DParameters)_cachedArgs[(int)NoiseTypes.Perlin]);
        _cachedNoises[(int)NoiseTypes.Cellular] = new CellularNoise2D((CellularNoise2DParameters)_cachedArgs[(int)NoiseTypes.Cellular]);
        _cachedNoises[(int)NoiseTypes.Simplex] = new SimplexNoise2D((SimplexNoise2DParameters)_cachedArgs[(int)NoiseTypes.Simplex]);
        _cachedNoises[(int)NoiseTypes.Value] = new ValueNoise2D((ValueNoise2DParameters)_cachedArgs[(int)NoiseTypes.Value]);
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
            float noise = CurrentNoise.Noise(x * freq, y * freq, (int)(repeat * freq));
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

    public NoiseParameters NoiseParameterAt(NoiseTypes type)
    {
        return _cachedArgs[(int)type];
    }

    public Noise2DLite Clone()
    {
        Noise2DLite clone = new Noise2DLite();
        clone.NoiseType = NoiseType;
        clone.Seed = Seed;
        clone.Octaves = Octaves;
        clone.Frequency = Frequency;
        clone.Inverted = Inverted;
        clone.Repeat = Repeat;
        clone.Size = Size;
        for (int i = 0; i < _cachedArgs.Length; i++)
        {
            _cachedArgs[i].CopyTo(clone._cachedArgs[i]);
            clone._cachedNoises[i].Seed = clone.Seed;
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

    NoiseParameters[] _cachedArgs = new NoiseParameters[Enum.GetValues<NoiseTypes>().Length];
    INoise2D[] _cachedNoises = new INoise2D[Enum.GetValues<NoiseTypes>().Length];
}