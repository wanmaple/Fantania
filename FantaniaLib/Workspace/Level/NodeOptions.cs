namespace FantaniaLib;

public struct NodeOptions
{
    public static readonly NodeOptions Default = new NodeOptions
    {
        Minimum = 1,
        Maximum = -1,
        DefaultOffset = new Vector2Int(64, 0),
    };

    public int Minimum;
    public int Maximum;
    public Vector2Int DefaultOffset;
}