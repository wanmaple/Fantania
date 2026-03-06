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

    private Vector3 _envLightDir = Vector3.Normalize(new Vector3(-1.0f, -4.0f, 1.0f));
    [SerializableField(FieldTypes.Vector3), EditableField(EditGroup = "G_LightingEnvironment", TooltipKey = "TT_LightingEnvironment_LightDirection")]
    public Vector3 EnvironmentLightDirection
    {
        get { return _envLightDir; }
        set
        {
            if (_envLightDir != value)
            {
                OnPropertyChanging(nameof(EnvironmentLightDirection));
                _envLightDir = value;
                OnPropertyChanged(nameof(EnvironmentLightDirection));
            }
        }
    }

    private float _envLightIntensity = 1.0f;
    [SerializableField(FieldTypes.Float), EditableField(EditGroup = "G_LightingEnvironment", TooltipKey = "TT_LightingEnvironment_LightIntensity")]
    public float EnvironmentLightIntensity
    {
        get { return _envLightIntensity; }
        set
        {
            if (_envLightIntensity != value)
            {
                OnPropertyChanging(nameof(EnvironmentLightIntensity));
                _envLightIntensity = value;
                OnPropertyChanged(nameof(EnvironmentLightIntensity));
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