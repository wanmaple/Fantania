namespace FantaniaLib;

public class TiledEntityManager
{
    public void AddEntity(IWorkspace workspace, TiledEntity entity)
    {
        Vector2Int tileSize = entity.GetTileSize(workspace);
        int layer = entity.Layer;
        var candidateGroups = _tileGroups.Where(g => g.Layer == layer && g.TileSize == tileSize && (g.BoundingBox.X - entity.Position.X) % tileSize.X == 0 && (g.BoundingBox.Y - entity.Position.Y) % tileSize.Y == 0);
        if (candidateGroups.Any())
        {
            // TODO: merge groups if possible.
        }
        else
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
        _tileGroups.RemoveFast(group);
        _guid2TileGroup.Remove(entity.GUID);
        // TODO: split groups if possible.
    }

    Dictionary<string, TileGroup> _guid2TileGroup = new Dictionary<string, TileGroup>();
    List<TileGroup> _tileGroups = new List<TileGroup>();
}