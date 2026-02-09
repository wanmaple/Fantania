using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public class TiledEntity : LevelEntity, ISelectableItem
{
    public override int NodeCount => 1;
    public int Depth => RealDepth;

    public Color SelectionColor => Colors.Orange;
    public Vector2 Anchor => new Vector2(0.5f, 0.5f);
    public Vector2 WorldPosition => Position.ToVector2();
    public int EntityOrder => Order;
    public int LocalOrder => 0;

    public Rectf BoundingBox => _aabb;

    private Vector2Int _size = new Vector2Int(4, 4);
    [SerializableField(FieldTypes.Vector2Int)]
    public Vector2Int Size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                OnPropertyChanging(nameof(Size));
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
    }

    internal TiledEntity()
    {
        for (int i = 0; i < _serializableFields.Count;)
        {
            if (_serializableFields[i].FieldName == nameof(Rotation) || _serializableFields[i].FieldName == nameof(Scale))
            {
                _serializableFields.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    public Vector2Int GetTileSize(IWorkspace workspace)
    {
        var placement = GetReferencedPlacement(workspace);
        return placement.Template.TileSize;
    }

    public override void OnEnter(IWorkspace workspace)
    {
        base.OnEnter(workspace);
        GUID = workspace.LevelModule.CurrentLevel!.ObtainGUID();
        workspace.LevelModule.CurrentLevel.TiledEntityManager.AddEntity(workspace, this);
    }

    public override void OnExit(IWorkspace workspace)
    {
        workspace.LevelModule.CurrentLevel!.TiledEntityManager.RemoveEntity(workspace, this);
        workspace.LevelModule.CurrentLevel.ReleaseGUID(GUID);
        base.OnExit(workspace);
    }

    public bool PointTest(Vector2 pt)
    {
        return true;
    }

    public bool CanTranslate(IWorkspace workspace)
    {
        return GetReferencedPlacement(workspace).Template.CanTranslate(0);
    }

    public bool CanRotate(IWorkspace workspace)
    {
        return false;
    }

    public bool CanScale(IWorkspace workspace)
    {
        return false;
    }

    public override Matrix3x3 TransformAt(int index)
    {
        return SelfTransform;
    }

    public override void GetLocalNodeAt(IWorkspace workspace, int index, out IReadOnlyList<LocalRenderInfo> locals)
    {
        if (Size.X * Size.Y <= 0)
            locals = Array.Empty<LocalRenderInfo>();
        else
        {
            var tiles = new List<LocalRenderInfo>(Size.X * Size.Y);
            var placement = GetReferencedPlacement(workspace);
            Vector2Int tileSize = GetTileSize(workspace);
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    var tileInfo = placement.Template.GetTileInfo(placement, Size, x, y);
                    var localInfo = new LocalRenderInfo
                    {
                        Stage = tileInfo.RenderStage,
                        Anchor = Vector2.Zero,
                        Position = new Vector2(x * tileSize.X, y * tileSize.Y),
                        Rotation = 0.0f,
                        Scale = Vector2.One,
                        Color = Vector4.One,
                        MaterialKey = tileInfo.MaterialKey,
                        Uniforms = tileInfo.Uniforms,
                        Sizer = new FixedSizer(tileSize),
                        Tiling = new Rectf(tileInfo.UVOffset, tileInfo.UVSize),
                    };
                    tiles.Add(localInfo);
                }
            }
            locals = tiles;
        }
    }

    public override void OnAddSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index, Rectf bound)
    {
        _localBound = bound;
        _aabb = new Rectf(Position.X + bound.X, Position.Y + bound.Y, bound.Width, bound.Height);
        bvh.AddItem(this);
        OnPropertyChanged(nameof(BoundingBox));
    }

    public override void OnRemoveSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh)
    {
        bvh.RemoveItem(this);
    }

    public override void OnUpdateSelectables(BoundingVolumeHierarchy<ISelectableItem> bvh, int index)
    {
        _aabb = new Rectf(Position.X + _localBound.X, Position.Y + _localBound.Y, _localBound.Width, _localBound.Height);
        bvh.UpdateItem(this);
        OnPropertyChanged(nameof(BoundingBox));
    }

    Rectf _localBound, _aabb;
}