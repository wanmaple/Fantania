using System.Numerics;
using System.Reflection;

namespace FantaniaLib;

public class LevelEntity : SyncableObject, ISerializableData, IEditableObject
{
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

    private int _depth = 49;
    [SerializableField(FieldTypes.Integer), EditableField(EditGroup = "G_Transform", TooltipKey = "TT_RelativeDepth", EditParameter = "0:99:1")]
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

    public int RealDepth => Layer * 100 + RelativeDepth;

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

    public IReadOnlyList<Vector2Int> Nodes => _nodes;

    public IReadOnlyList<FieldInfo> SerializableFields => _serializableFields;

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
        var props = GetPropertiesWithAttribute<SerializableFieldAttribute>();
        _serializableFields = new List<FieldInfo>(props.Count);
        foreach (PropertyInfo prop in props)
        {
            SerializableFieldAttribute attr = prop.GetCustomAttribute<SerializableFieldAttribute>()!;
            _serializableFields.Add(new FieldInfo
            {
                FieldName = prop.Name,
                FieldType = attr.FieldType,
            });
        }
    }

    internal UserPlacement? GetReferencedPlacement(IWorkspace workspace)
    {
        return workspace.DatabaseModule.GetTypedObject<UserPlacement>(PlacementReference.ReferenceType, PlacementReference.ReferenceID);
    }

    public void GetRenderables(IWorkspace workspace)
    {
        UserPlacement? placement = GetReferencedPlacement(workspace);
        if (placement != null)
        {
            IReadOnlyList<ScriptRenderInfo> rs = placement.Template.GetRenderables(this);
        }
    }

    public object? GetFieldValue(string fieldName)
    {
        var prop = GetType().GetProperty(fieldName);
        if (prop != null)
        {
            return prop.GetValue(this);
        }
        return null;
    }

    public void SetFieldValue(string fieldName, object? value)
    {
        var prop = GetType().GetProperty(fieldName);
        if (prop != null)
        {
            prop.SetValue(this, value);
        }
    }

    public IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var editableFields = new List<IEditableField>();
        var props = GetPropertiesWithAttribute<EditableFieldAttribute>();
        foreach (PropertyInfo prop in props)
        {
            var editableField = new SingleObjectEditableField(workspace, this, prop);
            editableFields.Add(editableField);
        }
        editableFields.Sort((f1, f2) => f1.FieldName.CompareTo(f2.FieldName));
        return editableFields;
    }

    List<FieldInfo> _serializableFields;
    List<Vector2Int> _nodes = new List<Vector2Int>(0);
}