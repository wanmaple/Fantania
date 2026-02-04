using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public class LevelEntityNode : SyncableObject, ISelectableItem
{
    public int NodeId { get; internal set; }

    public IMultiNodeContainer Owner => _container;
    public Color SelectionColor => Colors.Purple;
    public Vector2 Anchor => (_container.TransformAt(LocalOrder) * Vector2.Zero - _aabb.TopLeft) / _aabb.Size;
    public int Depth => _container.Depth;
    public int EntityOrder => _container.Order;
    public int LocalOrder => _container.AllNodes.IndexOf(this);

    private Rectf _aabb = Rectf.Zero;
    public Rectf BoundingBox
    {
        get { return _aabb; }
        set
        {
            if (_aabb != value)
            {
                _aabb = value;
                OnPropertyChanged(nameof(BoundingBox));
                OnPropertyChanged(nameof(Anchor));
            }
        }
    }

    public Rectf LocalBound { get; set; }

    private Vector2Int _position = Vector2Int.Zero;
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
                _container.MarkTransformDirty();
            }
        }
    }

    private float _rotation = 0.0f;
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
                _container.MarkTransformDirty();
            }
        }
    }

    private Vector2 _scale = Vector2.One;
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
                _container.MarkTransformDirty();
            }
        }
    }

    public LevelEntityNode(IMultiNodeContainer container)
    {
        _container = container;
    }

    public bool CanRotate(IWorkspace workspace)
    {
        return _container.CanRotate(workspace, LocalOrder);
    }

    public bool CanScale(IWorkspace workspace)
    {
        return _container.CanScale(workspace, LocalOrder);
    }

    public bool CanTranslate(IWorkspace workspace)
    {
        return _container.CanTranslate(workspace, LocalOrder);
    }

    public void OnTranslateBegin()
    {
        _startWorldPos = _container.TransformAt(LocalOrder) * Vector2.Zero;
    }

    public void OnTranslating(Vector2Int worldChange)
    {
        Matrix3x3 worldMat = _container.SelfTransform;
        Vector2 currentWorldPos = _startWorldPos + worldChange.ToVector2();
        Vector2 localPos = worldMat.Inverse() * currentWorldPos;
        Position = localPos.ToVector2i();
    }

    public bool PointTest(Vector2 pt)
    {
        return true;
    }

    public EntityNodeSnapshot CreateSnapshot()
    {
        return new EntityNodeSnapshot()
        {
            Position = Position,
            Rotation = Rotation,
            Scale = Scale,
        };
    }

    public void ApplySnapshot(EntityNodeSnapshot snapshot)
    {
        Position = snapshot.Position;
        Rotation = snapshot.Rotation;
        Scale = snapshot.Scale;
    }

    IMultiNodeContainer _container;
    Vector2 _startWorldPos;
}