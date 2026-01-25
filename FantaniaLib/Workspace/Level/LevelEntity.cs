using System.Numerics;

namespace FantaniaLib;

public class LevelEntity : BinaryObject
{
    public const int LAYER_RANGE = 100;
    public const int DEFAULT_RELATIVE_DEPTH = LAYER_RANGE / 2 - 1;
    public const int MIN_RELATIVE_DEPTH = 0;
    public const int MAX_RELATIVE_DEPTH = LAYER_RANGE - 1;
    public const string MIN_RELATIVE_DEPTH_STR = "0";
    public const string MAX_RELATIVE_DEPTH_STR = "99";

    [SerializableField(FieldTypes.String)]
    public string GUID { get; internal set; } = string.Empty;

    private Vector2 _anchor = Vector2.Zero;
    [SerializableField(FieldTypes.Vector2), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_Anchor")]
    public Vector2 Anchor
    {
        get { return _anchor; }
        set
        {
            if (_anchor != value)
            {
                OnPropertyChanging(nameof(Anchor));
                _anchor = value;
                OnPropertyChanged(nameof(Anchor));
            }
        }
    }
    
    private Vector2Int _position = Vector2Int.Zero;
    [SerializableField(FieldTypes.Vector2Int), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_Position")]
    public Vector2Int Position
    {
        get { return _position; }
        set
        {
            if (_position != value)
            {
                OnPropertyChanging(nameof(Position));
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }
    }

    private float _rotation = 0.0f;
    [SerializableField(FieldTypes.Float), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_Rotation")]
    public float Rotation
    {
        get { return _rotation; }
        set
        {
            if (_rotation != value)
            {
                OnPropertyChanging(nameof(Rotation));
                _rotation = value;
                OnPropertyChanged(nameof(Rotation));
            }
        }
    }

    private Vector2 _scale = Vector2.One;
    [SerializableField(FieldTypes.Vector2), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_Scale")]
    public Vector2 Scale
    {
        get { return _scale; }
        set
        {
            if (_scale != value)
            {
                OnPropertyChanging(nameof(Scale));
                _scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }
    }

    private int _layer = 0;
    [SerializableField(FieldTypes.Integer), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_Layer", EditParameter = "-40:40:1")]
    public int Layer
    {
        get { return _layer; }
        set
        {
            if (_layer != value)
            {
                OnPropertyChanging(nameof(Layer));
                _layer = value;
                OnPropertyChanged(nameof(Layer));
            }
        }
    }

    private int _depth = DEFAULT_RELATIVE_DEPTH;
    [SerializableField(FieldTypes.Integer), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_RelativeDepth", EditParameter = $"{MIN_RELATIVE_DEPTH_STR}:{MAX_RELATIVE_DEPTH_STR}:1")]
    public int RelativeDepth
    {
        get { return _depth; }
        set
        {
            if (_depth != value)
            {
                OnPropertyChanging(nameof(RelativeDepth));
                _depth = value;
                OnPropertyChanged(nameof(RelativeDepth));
            }
        }
    }

    public int RealDepth => Layer * LAYER_RANGE + RelativeDepth;

    private Vector4 _color = Vector4.One;
    [SerializableField(FieldTypes.Color), EditableField(EditGroup = "G_Appearance", TooltipKey = "TT_Color")]
    public Vector4 Color
    {
        get { return _color; }
        set
        {
            if (_color != value)
            {
                OnPropertyChanging(nameof(Color));
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
    }

    private TypeReference _refPlacement = TypeReference.None;
    [SerializableField(FieldTypes.TypeReference)]
    public TypeReference PlacementReference
    {
        get { return _refPlacement; }
        set
        {
            if (_refPlacement != value)
            {
                OnPropertyChanging(nameof(PlacementReference));
                _refPlacement = value;
                OnPropertyChanged(nameof(PlacementReference));
            }
        }
    }

    public IReadOnlyList<Vector2Int> Nodes { get; set; } = new List<Vector2Int>(0);

    public static LevelEntity BuildFromPlacement(UserPlacement placement)
    {
        var entity = new LevelEntity();
        entity.PlacementReference = new TypeReference(placement.TypeName, placement.ID);
        entity.Anchor = placement.Template.DefaultAnchor;
        entity.Layer = placement.Template.DefaultLayer;
        return entity;
    }

    internal LevelEntity()
    {
    }

    public UserPlacement? GetReferencedPlacement(IWorkspace workspace)
    {
        return workspace.DatabaseModule.GetTypedObject<UserPlacement>(PlacementReference.ReferenceType, PlacementReference.ReferenceID);
    }

    public bool GetLocalRenderInfo(IWorkspace workspace, out IReadOnlyList<LocalRenderInfo> locals)
    {
        bool ret = false;
        UserPlacement? placement = GetReferencedPlacement(workspace);
        if (placement != null)
        {
            ret = _nodesDirty || placement.FieldDirty;
            if (ret)
            {
                _cacheRenderInfo = placement.GetRenderInfo(Nodes);
                _nodesDirty = false;
            }
        }
        locals = _cacheRenderInfo;
        return ret;
    }

    bool _nodesDirty = true;
    IReadOnlyList<LocalRenderInfo> _cacheRenderInfo = Array.Empty<LocalRenderInfo>();
}