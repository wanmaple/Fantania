using System.Numerics;
using Avalonia.Media;

namespace FantaniaLib;

public class TiledEntity : LevelEntity, ISelectableItem, ISizeableEntity
{
    public override int NodeCount => 1;
    public int Depth => RealDepth;

    public Color SelectionColor => Colors.Orange;
    public Vector2 Anchor => new Vector2(0.0f, 0.0f);
    public Vector2 WorldPosition => Position.ToVector2();
    public int EntityOrder => Order;
    public int LocalOrder => 0;

    public Rectf BoundingBox => _aabb;

    private Vector2Int _size = Vector2Int.Zero;
    [SerializableField(FieldTypes.Vector2Int), EditableField(EditParameter = "1:100000:1", TooltipKey = "TT_TiledEntitySize")]
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
                RefreshSelf();
            }
        }
    }

    private int _seed = 0;
    [SerializableField(FieldTypes.Integer), EditableField(EditControlType = typeof(RandomSeedBox), TooltipKey = "TT_RandomSeed")]
    public int RandomSeed
    {
        get { return _seed; }
        set
        {
            if (_seed != value)
            {
                OnPropertyChanging(nameof(RandomSeed));
                _seed = value;
                OnPropertyChanged(nameof(RandomSeed));
                RefreshSelf();
            }
        }
    }

    internal TiledEntity()
    {
        RandomSeed = new Random().Next();
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

    public override IReadOnlyList<IEditableField> GetEditableFields(IWorkspace workspace)
    {
        var fields = (List<IEditableField>)base.GetEditableFields(workspace);
        if (!CanTranslate(workspace))
        {
            fields.RemoveAll(f => f.FieldName == nameof(Position));
        }
        if (!CanRotate(workspace))
        {
            fields.RemoveAll(f => f.FieldName == nameof(Rotation));
        }
        if (!CanScale(workspace))
        {
            fields.RemoveAll(f => f.FieldName == nameof(Scale));
        }
        return fields;
    }

    public Vector2Int GetTileSize(IWorkspace workspace)
    {
        var placement = GetReferencedPlacement(workspace);
        return placement.Template.TileSize;
    }

    public Vector2Int GetUnitSize(IWorkspace workspace)
    {
        return GetTileSize(workspace);
    }

    public override void OnLoaded(IWorkspace workspace, Level level)
    {
        base.OnLoaded(workspace, level);
        level.TiledEntityManager.AddEntity(workspace, this);
    }

    public override void OnEnter(IWorkspace workspace)
    {
        base.OnEnter(workspace);
        GUID = workspace.LevelModule.CurrentLevel!.ObtainGUID();
        workspace.LevelModule.CurrentLevel.TiledEntityManager.AddEntity(workspace, this);
        var mgr = workspace.LevelModule.CurrentLevel.TiledEntityManager;
        var group = mgr.GetGroup(this);
        foreach (var e in group.Entities)
        {
            e.RefreshSelf();
        }
    }

    public override void OnExit(IWorkspace workspace)
    {
        var mgr = workspace.LevelModule.CurrentLevel!.TiledEntityManager;
        var group = mgr.GetGroup(this);
        foreach (var e in group.Entities)
        {
            if (e != this)
                e.RefreshSelf();
        }
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
                    int hash = Hash(x, y, RandomSeed);
                    TileLocationTypes locType = workspace.LevelModule.CurrentLevel!.TiledEntityManager.GetLocationType(workspace, this, x, y);
                    var tileInfo = placement.Template.GetTileInfo(placement, Size, locType, hash);
                    var localInfo = new LocalRenderInfo
                    {
                        Stage = tileInfo.RenderStage,
                        Anchor = Vector2.Zero,
                        Position = new Vector2(x * tileSize.X, y * tileSize.Y),
                        Rotation = 0.0f,
                        Scale = Vector2.One,
                        Color = tileInfo.Color,
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

    public override void OnUpdateSnaps(IWorkspace workspace, BoundingVolumeHierarchy<ISelectableItem> bvh, Vector2 worldPos)
    {
        Vector2Int tileSize = GetTileSize(workspace);
        Rectf rect = new Rectf(worldPos - tileSize.ToVector2() * 0.5f, tileSize.ToVector2());
        var closeEntities = new List<ISelectableItem>(8);
        bvh.RectTest(rect, closeEntities, s => s is TiledEntity e && workspace.LevelModule.CurrentLevel!.TiledEntityManager.IsEntitySnappable(workspace, this, e));
        var snaps = workspace.EditorModule.SnapPoints;
        ISnapPoint? nearest = null;
        float nearestDisSq = float.MaxValue;
        foreach (TiledEntity e in closeEntities)
        {
            int left = e.Position.X;
            int right = e.Position.X + e.Size.X * tileSize.X;
            int top = e.Position.Y;
            int bottom = e.Position.Y + e.Size.Y * tileSize.Y;
            if (MathF.Abs(left - worldPos.X) <= tileSize.X * 0.5f)
            {
                int yOffset = MathHelper.FloorToInt(MathF.Abs(e.Position.Y - worldPos.Y) / tileSize.Y);
                for (int i = 0; i < 2; i++)
                {
                    int y = e.Position.Y + (yOffset + i) * tileSize.Y;
                    if (y < top || y > bottom)
                        continue;
                    Vector2 snapPos = new Vector2(left, y);
                    var snapPt = new TiledSnapPoint
                    {
                        Position = snapPos,
                    };
                    float disSq = Vector2.DistanceSquared(worldPos, snapPos);
                    if (disSq < nearestDisSq)
                    {
                        nearestDisSq = disSq;
                        nearest = snapPt;
                    }
                }
            }
            if (MathF.Abs(right - worldPos.X) <= tileSize.X * 0.5f)
            {
                int yOffset = MathHelper.FloorToInt(MathF.Abs(e.Position.Y - worldPos.Y) / tileSize.Y);
                for (int i = 0; i < 2; i++)
                {
                    int y = e.Position.Y + (yOffset + i) * tileSize.Y;
                    if (y < top || y > bottom)
                        continue;
                    Vector2 snapPos = new Vector2(right, y);
                    var snapPt = new TiledSnapPoint
                    {
                        Position = snapPos,
                    };
                    float disSq = Vector2.DistanceSquared(worldPos, snapPos);
                    if (disSq < nearestDisSq)
                    {
                        nearestDisSq = disSq;
                        nearest = snapPt;
                    }
                }
            }
            if (MathF.Abs(top - worldPos.Y) <= tileSize.Y * 0.5f)
            {
                int xOffset = MathHelper.FloorToInt(MathF.Abs(e.Position.X - worldPos.X) / tileSize.X);
                for (int i = 0; i < 2; i++)
                {
                    int x = e.Position.X + (xOffset + i) * tileSize.X;
                    if (x < left || x > right)
                        continue;
                    Vector2 snapPos = new Vector2(x, top);
                    var snapPt = new TiledSnapPoint
                    {
                        Position = snapPos,
                    };
                    float disSq = Vector2.DistanceSquared(worldPos, snapPos);
                    if (disSq < nearestDisSq)
                    {
                        nearestDisSq = disSq;
                        nearest = snapPt;
                    }
                }
            }
            if (MathF.Abs(bottom - worldPos.Y) <= tileSize.Y * 0.5f)
            {
                int xOffset = MathHelper.FloorToInt(MathF.Abs(e.Position.X - worldPos.X) / tileSize.X);
                for (int i = 0; i < 2; i++)
                {
                    int x = e.Position.X + (xOffset + i) * tileSize.X;
                    if (x < left || x > right)
                        continue;
                    Vector2 snapPos = new Vector2(x, bottom);
                    var snapPt = new TiledSnapPoint
                    {
                        Position = snapPos,
                    };
                    float disSq = Vector2.DistanceSquared(worldPos, snapPos);
                    if (disSq < nearestDisSq)
                    {
                        nearestDisSq = disSq;
                        nearest = snapPt;
                    }
                }
            }
        }
        if (nearest != null && (snaps.Count == 0 || !nearest.Equals(snaps[0])))
        {
            snaps.Clear();
            nearest.IsActive = true;
            snaps.Add(nearest);
            workspace.EditorModule.Notify();
        }
        else if (nearest == null && snaps.Count > 0)
        {
            snaps.Clear();
            workspace.EditorModule.Notify();
        }
    }

    int Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint s = (uint)seed;
            uint h = (uint)x + 0x9E3779B9u;
            h ^= (h << 6);
            h ^= (h >> 2);
            h = (h + (uint)y);
            h ^= (h << 7);
            h ^= (h >> 3);
            h = (h + s);
            h ^= (h << 13);
            h ^= (h >> 11);
            return (int)(h & 0x7FFFFFFFu);
        }
    }

    Rectf _localBound, _aabb;
}