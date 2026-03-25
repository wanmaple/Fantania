namespace FantaniaLib;

[BindingScript]
public enum TileLocationTypes
{
    Center,                    // 111-111-111
    CornerOuterTopLeft,        // x0x-011-x1x
    CornerOuterTopRight,       // x0x-110-x1x
    CornerOuterBottomLeft,     // x1x-011-x0x
    CornerOuterBottomRight,    // x1x-110-x0x
    CornerInnerTopLeft,        // 011-111-111
    CornerInnerTopRight,       // 110-111-111
    CornerInnerBottomLeft,     // 111-111-011
    CornerInnerBottomRight,    // 111-111-110
    EdgeTop,                   // x1x-111-x0x
    EdgeBottom,                // x0x-111-x1x
    EdgeLeft,                  // x1x-011-x1x
    EdgeRight,                 // x1x-110-x1x
    Single,                    // x0x-010-x0x
    PillarHorizontalLeft,      // x0x-011-x0x
    PillarHorizontalRight,     // x0x-110-x0x
    PillarHorizontalCenter,    // x0x-111-x0x
    PillarVerticalTop,         // x0x-010-x1x
    PillarVerticalBottom,      // x1x-010-x0x
    PillarVerticalCenter,      // x1x-010-x1x
    InnerDiagonalSlash,        // 110-111-011
    InnerDiagonalBackslash,    // 011-111-110
    InnerX,                    // 010-111-010
    InnerMissingTopLeft,       // 110-111-010
    InnerMissingTopRight,      // 011-111-010
    InnerMissingBottomLeft,    // 010-111-110
    InnerMissingBottomRight,   // 010-111-011
    InnerProtrudingTop,        // 010-111-111
    InnerProtrudingBottom,     // 111-111-010
    InnerProtrudingLeft,       // 011-111-011
    InnerProtrudingRight,      // 110-111-110
}

public class TileGroup
{
    public int Layer { get; private set; }
    public Vector2Int TileSize { get; private set; }
    public Recti BoundingBox { get; private set; } = Recti.Zero;

    public bool IsEmpty => _entities.Count == 0;
    public IReadOnlyList<TiledEntity> Entities => _entities;

    public TileGroup(Vector2Int tileSize, int layer)
    {
        TileSize = tileSize;
        Layer = layer;
    }

    public byte GetTileMask(TiledEntity entity, int x, int y)
    {
        int worldX = entity.Position.X + x * TileSize.X;
        int worldY = entity.Position.Y + y * TileSize.Y;
        byte mask = 0;   // 8-bits 从高位到低位 TL-T-TR-L-R-BL-B-BR
        foreach (var e in _entities)
        {
            int dx = worldX - e.Position.X;
            int dy = worldY - e.Position.Y;
            int tilesX = dx / TileSize.X;
            int tilesY = dy / TileSize.Y;
            int shift = 7;
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if (i == 0 && j == 0) continue;
                    int tx = tilesX + i;
                    int ty = tilesY + j;
                    if (tx >= 0 && tx < e.Size.X && ty >= 0 && ty < e.Size.Y)
                    {
                        mask |= (byte)(1 << shift);
                    }
                    --shift;
                }
            }
        }
        return mask;
    }

    public void AddEntity(TiledEntity entity)
    {
        _entities.Add(entity);
        BoundingBox = BoundingBox.Merge(new Recti(entity.Position.X, entity.Position.Y, entity.Size.X * TileSize.X, entity.Size.Y * TileSize.Y));
    }

    public void RemoveEntity(TiledEntity entity)
    {
        _entities.Remove(entity);
        BoundingBox = Recti.Zero;
        foreach (var e in _entities)
        {
            BoundingBox = BoundingBox.Merge(new Recti(e.Position.X, e.Position.Y, e.Size.X * TileSize.X, e.Size.Y * TileSize.Y));
        }
    }

    public void Merge(TileGroup other)
    {
        foreach (var e in other._entities)
        {
            _entities.Add(e);
        }
        BoundingBox = BoundingBox.Merge(other.BoundingBox);
    }

    public TiledEntity? GetTiledEntityAtGrid(int xGrid, int yGrid)
    {
        foreach (var e in _entities)
        {
            int eX = (e.Position.X - BoundingBox.X) / TileSize.X;
            int eY = (e.Position.Y - BoundingBox.Y) / TileSize.Y;
            if (xGrid >= eX && xGrid < eX + e.Size.X && yGrid >= eY && yGrid < eY + e.Size.Y)
            {
                return e;
            }
        }
        return null;
    }

    public TiledEntity? GetTiledEntityAtWorld(int xWorld, int yWorld)
    {
        foreach (var e in _entities)
        {
            if (xWorld >= e.Position.X && xWorld < e.Position.X + e.Size.X * TileSize.X &&
                yWorld >= e.Position.Y && yWorld < e.Position.Y + e.Size.Y * TileSize.Y)
            {
                return e;
            }
        }
        return null;
    }

    List<TiledEntity> _entities = new List<TiledEntity>(0);
}