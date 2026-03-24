namespace FantaniaLib;

public class RidgedMultiFractalCompositorParameters : NoiseCompositorParameters
{
    private float _offset = 1.0f;
    [EditableField(EditParameter = "0.0:2.0:0.01", TooltipKey = "TT_RidgedMultiFractalOffset")]
    public float Offset
    {
        get { return _offset; }
        set
        {
            if (_offset != value)
            {
                _offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }
    }

    private float _gain = 2.0f;
    [EditableField(EditParameter = "0.0:4.0:0.01", TooltipKey = "TT_RidgedMultiFractalGain")]
    public float Gain
    {
        get { return _gain; }
        set
        {
            if (_gain != value)
            {
                _gain = value;
                OnPropertyChanged(nameof(Gain));
            }
        }
    }

    private float _ridgeExponent = 2.0f;
    [EditableField(EditParameter = "1.0:8.0:0.1", TooltipKey = "TT_RidgedMultiFractalExponent")]
    public float RidgeExponent
    {
        get { return _ridgeExponent; }
        set
        {
            if (_ridgeExponent != value)
            {
                _ridgeExponent = value;
                OnPropertyChanged(nameof(RidgeExponent));
            }
        }
    }
}

public class RidgedMultiFractalCompositor : INoise2DCompositor
{
    public RidgedMultiFractalCompositorParameters Arguments { get; private set; }

    public RidgedMultiFractalCompositor(RidgedMultiFractalCompositorParameters args)
    {
        Arguments = args;
    }

    public float Composite(INoise2D noise, float x, float y, int repeat)
    {
        float n = noise.Noise(x, y, repeat);
        float signal = Arguments.Offset - MathF.Abs(n);
        signal = MathF.Max(0.0f, signal);
        signal = MathF.Pow(signal, Arguments.RidgeExponent);
        signal *= Arguments.Gain;
        signal = MathHelper.Clamp(signal, 0.0f, 1.0f);
        return signal * 2.0f - 1.0f;
    }
}
