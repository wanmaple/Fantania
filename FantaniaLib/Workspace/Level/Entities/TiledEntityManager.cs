namespace FantaniaLib;

public class TiledEntityManager
{
    public IReadOnlyList<TileGroup> TileGroups => _tileGroups;

    public TileGroup GetGroup(TiledEntity entity)
    {
        return _guid2TileGroup[entity.GUID];
    }

    public TileLocationTypes GetLocationType(IWorkspace workspace, TiledEntity entity, int x, int y)
    {
        if (!_guid2TileGroup.TryGetValue(entity.GUID, out var group))
        {
            // 这一定是一个Ghost Entity
            return GetDefaultLocationType(x, y, entity.Size);
        }
        else
        {
            byte mask = group.GetTileMask(entity, x, y);
            if (mask == 0x7F)
                return TileLocationTypes.CornerInnerTopLeft;
            if (mask == 0xDF)
                return TileLocationTypes.CornerInnerTopRight;
            if (mask == 0xFB)
                return TileLocationTypes.CornerInnerBottomLeft;
            if (mask == 0xFE)
                return TileLocationTypes.CornerInnerBottomRight;
            if (mask == 0xDB)
                return TileLocationTypes.InnerDiagonalSlash;
            if (mask == 0x7E)
                return TileLocationTypes.InnerDiagonalBackslash;
            if (mask == 0x5A)
                return TileLocationTypes.InnerX;
            if (mask == 0xDA)
                return TileLocationTypes.InnerMissingTopLeft;
            if (mask == 0x7A)
                return TileLocationTypes.InnerMissingTopRight;
            if (mask == 0x5B)
                return TileLocationTypes.InnerMissingBottomRight;
            if (mask == 0x5E)
                return TileLocationTypes.InnerMissingBottomLeft;
            if (mask == 0x5F)
                return TileLocationTypes.InnerProtrudingTop;
            if (mask == 0xFA)
                return TileLocationTypes.InnerProtrudingBottom;
            if (mask == 0x7B)
                return TileLocationTypes.InnerProtrudingLeft;
            if (mask == 0xDE)
                return TileLocationTypes.InnerProtrudingRight;
            if ((mask & 0x52) == 0 && (mask & 0x08) == 0x08)
                return TileLocationTypes.PillarHorizontalLeft;
            if ((mask & 0x4A) == 0 && (mask & 0x10) == 0x10)
                return TileLocationTypes.PillarHorizontalRight;
            if ((mask & 0x42) == 0 && (mask & 0x18) == 0x18)
                return TileLocationTypes.PillarHorizontalCenter;
            if ((mask & 0x58) == 0 && (mask & 0x02) == 0x02)
                return TileLocationTypes.PillarVerticalTop;
            if ((mask & 0x1A) == 0 && (mask & 0x40) == 0x40)
                return TileLocationTypes.PillarVerticalBottom;
            if ((mask & 0x18) == 0 && (mask & 0x42) == 0x42)
                return TileLocationTypes.PillarVerticalCenter;
            if ((mask & 0x40) == 0 && (mask & 0x1A) == 0x1A)
                return TileLocationTypes.EdgeTop;
            if ((mask & 0x02) == 0 && (mask & 0x58) == 0x58)
                return TileLocationTypes.EdgeBottom;
            if ((mask & 0x10) == 0 && (mask & 0x4A) == 0x4A)
                return TileLocationTypes.EdgeLeft;
            if ((mask & 0x08) == 0 && (mask & 0x52) == 0x52)
                return TileLocationTypes.EdgeRight;
            if ((mask & 0x50) == 0 && (mask & 0x0A) == 0x0A)
                return TileLocationTypes.CornerOuterTopLeft;
            if ((mask & 0x48) == 0 && (mask & 0x12) == 0x12)
                return TileLocationTypes.CornerOuterTopRight;
            if ((mask & 0x12) == 0 && (mask & 0x48) == 0x48)
                return TileLocationTypes.CornerOuterBottomLeft;
            if ((mask & 0x0A) == 0 && (mask & 0x50) == 0x50)
                return TileLocationTypes.CornerOuterBottomRight;
            if ((mask & 0x5A) == 0)
                return TileLocationTypes.Single;
            return TileLocationTypes.Center;
        }
    }

    public bool IsEntitySnappable(IWorkspace workspace, TiledEntity srcEntity, TiledEntity dstEntity)
    {
        if (srcEntity.GUID == dstEntity.GUID)
            return false;
        if (!_guid2TileGroup.TryGetValue(dstEntity.GUID, out var group))
            return false;
        if (group.TileSize != srcEntity.GetTileSize(workspace))
            return false;
        if (group.Layer != srcEntity.Layer)
            return false;
        return true;
    }

    public void AddEntity(IWorkspace workspace, TiledEntity entity)
    {
        Vector2Int tileSize = entity.GetTileSize(workspace);
        int layer = entity.Layer;
        var candidateGroups = GetCandidateTileGroups(entity, workspace);
        bool canMerge = false;
        if (candidateGroups.Any())
        {
            var touching = candidateGroups.Where(g => g.Entities.Any(e => EntitiesAdjacent(entity, e, tileSize))).ToArray();
            if (touching.Length > 0)
            {
                canMerge = true;
                var mainGroup = touching[0];
                mainGroup.AddEntity(entity);
                _guid2TileGroup.Add(entity.GUID, mainGroup);
                for (int i = 1; i < touching.Length; i++)
                {
                    var group = touching[i];
                    foreach (var e in group.Entities)
                    {
                        _guid2TileGroup[e.GUID] = mainGroup;
                    }
                    mainGroup.Merge(group);
                    _tileGroups.Remove(group);
                }
            }
        }
        if (!canMerge)
        {
            var group = new TileGroup(tileSize, layer);
            group.AddEntity(entity);
            _tileGroups.Add(group);
            _guid2TileGroup.Add(entity.GUID, group);
        }
    }

    public void RemoveEntity(IWorkspace workspace, TiledEntity entity)
    {
        var group = _guid2TileGroup[entity.GUID];
        group.RemoveEntity(entity);
        _guid2TileGroup.Remove(entity.GUID);
        if (group.IsEmpty)
            _tileGroups.Remove(group);
        else
        {
            var remainingEntities = group.Entities.ToList();
            var components = FindConnectedComponents(remainingEntities, group.TileSize);
            if (components.Count > 1)
            {
                _tileGroups.Remove(group);
                foreach (var component in components)
                {
                    var newGroup = new TileGroup(group.TileSize, group.Layer);
                    foreach (var e in component)
                    {
                        newGroup.AddEntity(e);
                        _guid2TileGroup[e.GUID] = newGroup;
                    }
                    _tileGroups.Add(newGroup);
                }
            }
        }
    }

    TileGroup[] GetCandidateTileGroups(TiledEntity entity, IWorkspace workspace)
    {
        Vector2Int tileSize = entity.GetTileSize(workspace);
        int layer = entity.Layer;
        return _tileGroups.Where(g => g.Layer == layer && g.TileSize == tileSize && (g.BoundingBox.X - entity.Position.X) % tileSize.X == 0 && (g.BoundingBox.Y - entity.Position.Y) % tileSize.Y == 0).ToArray();
    }

    List<List<TiledEntity>> FindConnectedComponents(List<TiledEntity> entities, Vector2Int tileSize)
    {
        var visited = new HashSet<TiledEntity>();
        var components = new List<List<TiledEntity>>();
        foreach (var entity in entities)
        {
            if (!visited.Contains(entity))
            {
                var component = new List<TiledEntity>();
                var queue = new Queue<TiledEntity>();
                queue.Enqueue(entity);
                visited.Add(entity);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    component.Add(current);
                    foreach (var neighbor in entities.Where(e => !visited.Contains(e) && EntitiesAdjacent(current, e, tileSize)))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
                components.Add(component);
            }
        }
        return components;
    }

    bool EntitiesAdjacent(TiledEntity a, TiledEntity b, Vector2Int tileSize)
    {
        int ax1 = a.Position.X;
        int ay1 = a.Position.Y;
        int ax2 = ax1 + a.Size.X * tileSize.X;
        int ay2 = ay1 + a.Size.Y * tileSize.Y;
        int bx1 = b.Position.X;
        int by1 = b.Position.Y;
        int bx2 = bx1 + b.Size.X * tileSize.X;
        int by2 = by1 + b.Size.Y * tileSize.Y;
        bool intersect = ax1 < bx2 && ax2 > bx1 && ay1 < by2 && ay2 > by1;
        if (intersect) return true;
        bool verticalOverlap = ay1 < by2 && ay2 > by1;
        bool horizontalOverlap = ax1 < bx2 && ax2 > bx1;
        if (ax2 == bx1 && verticalOverlap) return true; // A right == B left
        if (bx2 == ax1 && verticalOverlap) return true; // B right == A left
        if (ay2 == by1 && horizontalOverlap) return true; // A bottom == B top
        if (by2 == ay1 && horizontalOverlap) return true; // B bottom == A top
        return false;
    }

    TileLocationTypes GetDefaultLocationType(int x, int y, Vector2Int size)
    {
        if (size.X == 1 && size.Y == 1)
            return TileLocationTypes.Single;
        bool left = x == 0;
        bool right = x == size.X - 1;
        bool top = y == 0;
        bool bottom = y == size.Y - 1;
        if (size.X == 1)
        {
            if (top)
                return TileLocationTypes.PillarVerticalTop;
            else if (bottom)
                return TileLocationTypes.PillarVerticalBottom;
            return TileLocationTypes.PillarVerticalCenter;
        }
        if (size.Y == 1)
        {
            if (left)
                return TileLocationTypes.PillarHorizontalLeft;
            else if (right)
                return TileLocationTypes.PillarHorizontalRight;
            return TileLocationTypes.PillarHorizontalCenter;
        }
        if (top && left)
            return TileLocationTypes.CornerOuterTopLeft;
        else if (top && right)
            return TileLocationTypes.CornerOuterTopRight;
        else if (bottom && left)
            return TileLocationTypes.CornerOuterBottomLeft;
        else if (bottom && right)
            return TileLocationTypes.CornerOuterBottomRight;
        else if (top)
            return TileLocationTypes.EdgeTop;
        else if (bottom)
            return TileLocationTypes.EdgeBottom;
        else if (left)
            return TileLocationTypes.EdgeLeft;
        else if (right)
            return TileLocationTypes.EdgeRight;
        return TileLocationTypes.Center;
    }

    Dictionary<string, TileGroup> _guid2TileGroup = new Dictionary<string, TileGroup>();
    List<TileGroup> _tileGroups = new List<TileGroup>();
}