using System.Numerics;

namespace FantaniaLib;

public class LevelMetadata : FantaniaObject
{
    private Vector4 _ambient = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
    [SerializableField(FieldTypes.Color), EditableField(EditGroup = "G_LightingEnvironment", TooltipKey = "TT_LightingEnvironment_AmbientColor")]
    public Vector4 AmbientColor
    {
        get { return _ambient; }
        set
        {
            if (_ambient != value)
            {
                OnPropertyChanging(nameof(AmbientColor));
                _ambient = value;
                OnPropertyChanged(nameof(AmbientColor));
            }
        }
    }

    private Direction3D _sunlightDir = new Direction3D { Azimuth = 45.0f, Elevation = 30.0f };
    [SerializableField(FieldTypes.Direction3D), EditableField(EditGroup = "G_LightingEnvironment", TooltipKey = "TT_LightingEnvironment_LightDirection")]
    public Direction3D SunLightDirection
    {
        get { return _sunlightDir; }
        set
        {
            if (_sunlightDir != value)
            {
                OnPropertyChanging(nameof(SunLightDirection));
                _sunlightDir = value;
                OnPropertyChanged(nameof(SunLightDirection));
            }
        }
    }

    private Vector4 _sunlightColor = Vector4.One;
    [SerializableField(FieldTypes.Color), EditableField(EditGroup = "G_LightingEnvironment", TooltipKey = "TT_LightingEnvironment_LightColor")]
    public Vector4 SunlightColor
    {
        get { return _sunlightColor; }
        set
        {
            if (_sunlightColor != value)
            {
                OnPropertyChanging(nameof(SunlightColor));
                _sunlightColor = value;
                OnPropertyChanged(nameof(SunlightColor));
            }
        }
    }

    private float _sunlightIntensity = 1.0f;
    [SerializableField(FieldTypes.Float), EditableField(EditGroup = "G_LightingEnvironment", TooltipKey = "TT_LightingEnvironment_LightIntensity")]
    public float SunLightIntensity
    {
        get { return _sunlightIntensity; }
        set
        {
            if (_sunlightIntensity != value)
            {
                OnPropertyChanging(nameof(SunLightIntensity));
                _sunlightIntensity = value;
                OnPropertyChanged(nameof(SunLightIntensity));
            }
        }
    }

    public override string OnCopy(IWorkspace workspace)
    {
        return string.Empty;
    }

    public override void OnPaste(IWorkspace workspace, string serializedData)
    {
    }
}