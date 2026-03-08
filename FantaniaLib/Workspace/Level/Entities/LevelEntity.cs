using System.Collections.ObjectModel;
using System.Numerics;

namespace FantaniaLib;

public abstract class LevelEntity : BinaryObject
{
    public event Action<LevelEntity>? RenderingDirty;

    public const int LAYER_RANGE = 100;
    public const int DEFAULT_RELATIVE_DEPTH = LAYER_RANGE / 2 - 1;
    public const int MIN_RELATIVE_DEPTH = 0;
    public const int MAX_RELATIVE_DEPTH = LAYER_RANGE - 1;
    public const string MIN_RELATIVE_DEPTH_STR = "0";
    public const string MAX_RELATIVE_DEPTH_STR = "99";

    public abstract int NodeCount { get; }

    public bool PlacementDirty { get; set; }
    public virtual Matrix3x3 SelfTransform => MathHelper.BuildTransform(Vector2.Zero, Vector2.Zero, Position.ToVector2(), Rotation, Scale);

    [SerializableField(FieldTypes.String)]
    public string GUID { get; internal set; } = string.Empty;

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
                RaiseRenderingDirty();
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
                RaiseRenderingDirty();
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
                RaiseRenderingDirty();
            }
        }
    }

    private int _layer = 0;
    [SerializableField(FieldTypes.Integer), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_Layer", EditParameter = "-40:40:1", EditControlType = typeof(LayerBox))]
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
                RaiseRenderingDirty();
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
                RaiseRenderingDirty();
            }
        }
    }

    public int RealDepth => Layer * LAYER_RANGE + RelativeDepth;

    private TypeReference _refPlacement = TypeReference.None;
    [SerializableField(FieldTypes.TypeReference), EditableField(EditControlType = typeof(ReadonlyTypeReference))]
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

    private int _order = 0;
    [SerializableField(FieldTypes.Integer)]
    public int Order
    {
        get { return _order; }
        set
        {
            if (_order != value)
            {
                OnPropertyChanging(nameof(Order));
                _order = value;
                OnPropertyChanged(nameof(Order));
                RaiseRenderingDirty();
            }
        }
    }

    public static LevelEntity BuildFromPlacement(IWorkspace workspace, UserPlacement placement)
    {
        LevelEntity? entity;
        if (placement.Template.PlacementType == PlacementTypes.MultiNodes)
            entity = new MultiNodesEntity();
        else if (placement.Template.PlacementType == PlacementTypes.Tiled)
            entity = new TiledEntity();
        else if (placement.Template.PlacementType == PlacementTypes.Single)
            entity = new SingleNodeEntity();
        else if (placement.Template.PlacementType == PlacementTypes.LightSource)
            entity = new LightSourceEntity();
        else
            entity = new SingleNodeEntity();
        entity.PlacementReference = new TypeReference(placement.TypeName, placement.ID);
        entity.Layer = placement.Template.DefaultLayer;
        entity.Initialize(workspace);
        return entity;
    }

    protected LevelEntity()
    {
    }

    public virtual void Initialize(IWorkspace workspace)
    {}

    /// <summary>
    /// 这个周期是在加载Level时已经存在的Entity会调用，这个时候的Entity不会走OnEnter。
    /// </summary>
    public virtual void OnLoaded(IWorkspace workspace, Level level)
    {
        GetReferencedPlacement(workspace).FieldChanged += OnPlacementChanged;
    }

    public virtual void OnEnter(IWorkspace workspace)
    {
        GetReferencedPlacement(workspace).FieldChanged += OnPlacementChanged;
    }

    public virtual void OnExit(IWorkspace workspace)
    {
        GetReferencedPlacement(workspace).FieldChanged -= OnPlacementChanged;
    }

    void OnPlacementChanged()
    {
        RefreshSelf();
    }

    protected void RaiseRenderingDirty()
    {
        RenderingDirty?.Invoke(this);
    }

    public void RefreshSelf()
    {
        PlacementDirty = true;
        RaiseRenderingDirty();
    }

    public UserPlacement GetReferencedPlacement(IWorkspace workspace)
    {
        var placement = workspace.DatabaseModule.GetTypedObject<UserPlacement>(PlacementReference.ReferenceType, PlacementReference.ReferenceID);
        if (placement == null)
            placement = new UserPlacement(workspace.PlacementModule.FallbackTemplate!, -1);
        return placement;
    }

    public virtual void OnTranslateBegin()
    {
        _startWorldPos = Position;
    }

    public virtual void OnTranslating(Vector2Int worldChange)
    {
        Position = _startWorldPos + worldChange;
    }

    public virtual void OnRotateBegin()
    {
    }

    public virtual void OnRotating(float rotationChange)
    {
        Rotation += rotationChange;
    }

    public virtual void OnScaleBegin()
    {
        _startScale = Scale;
    }

    public virtual void OnScaling(Vector2 scaleFactor)
    {
        Scale = new Vector2(_startScale.X * scaleFactor.X, _startScale.Y * scaleFactor.Y);
    }

    public virtual void OnUpdateSnaps(IWorkspace workspace, BoundingVolumeHierarchy<ISelectableItem> bvh, Vector2 worldPos)
    {
    }

    public abstract Matrix3x3 TransformAt(int index);
    public abstract void GetLocalNodeAt(IWorkspace workspace, int index, out IReadOnlyList<LocalRenderInfo> locals);
    public abstract void OnAddSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index, Rectf bound);
    public abstract void OnRemoveSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh);
    public abstract void OnUpdateSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index);

    public virtual void GetBackgroundNodes(IWorkspace workspace, out IReadOnlyList<LocalRenderInfo> locals)
    {
        locals = Array.Empty<LocalRenderInfo>();
    }
    public virtual void GetForegroundNodes(IWorkspace workspace, out IReadOnlyList<LocalRenderInfo> locals)
    {
        locals = Array.Empty<LocalRenderInfo>();
    }
    public virtual int GetIndexByNodeId(int nodeId) => -1;

    Vector2Int _startWorldPos;
    Vector2 _startScale;
}