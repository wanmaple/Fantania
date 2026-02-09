namespace FantaniaLib;

public class TileGroup
{
    public int Layer { get; private set; }
    public Vector2Int TileSize { get; private set; }
    public Recti BoundingBox { get; private set; }

    public TileGroup(Vector2Int tileSize, int layer)
    {
        TileSize = tileSize;
        Layer = layer;
    }

    public bool IsEntityAdjacent(TiledEntity entity)
    {
        return false;
    }

    public void AddEntity(TiledEntity entity)
    {
    }

    public void RemoveEntity(TiledEntity entity)
    {
    }

    public void Merge(TileGroup other)
    {
    }
}